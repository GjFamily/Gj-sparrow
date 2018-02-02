//
//  MediaTools.m
//  Unity-iPhone
//
//  Created by 高杰 on 2017/8/12.
//

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <MediaPlayer/MediaPlayer.h>
#import <UIKit/UIKit.h>
#import <OpenGLES/EAGL.h>
#import <OpenGLES/ES1/gl.h>
#import <OpenGLES/ES1/glext.h>
#import <OpenGLES/ES2/gl.h>
#import <OpenGLES/ES2/glext.h>
#import <OpenGLES/ES3/gl.h>
#import <OpenGLES/ES3/glext.h>
#import <OpenGLES/EAGLDrawable.h>
#import <QuartzCore/QuartzCore.h>
#import <Metal/Metal.h>
#import "UnityAppController.h"
#import "UnityAppController+Rendering.h"
#import "Unity/DisplayManager.h"
#import "Unity/UnityRendering.h"
#import "UI/SplashScreen.h"

typedef void (*UNITY_AUDIO_ENCODE_ERROR_CALLBACK)();
typedef void (*UNITY_VIDEO_ENCODE_ERROR_CALLBACK)();

typedef void (*UNITY_RECORD_PROCESS_CALLBACK)(double time);
typedef void (*UNITY_RECORD_FINISH_CALLBACK)(bool success);

typedef void (*UNITY_MEDIA_IMAGE_CALLBACK)();

@interface MediaToolsSession : NSObject<AVAudioRecorderDelegate,AVCaptureVideoDataOutputSampleBufferDelegate,AVCaptureAudioDataOutputSampleBufferDelegate, CAAnimationDelegate>{
@public
    UNITY_RECORD_FINISH_CALLBACK _finish_callback;
    UNITY_RECORD_PROCESS_CALLBACK _process_callback;
@private
    CMTime _timeOffset;//录制的偏移CMTime
    CMTime _lastVideo;//记录上一次视频数据文件的CMTime
    CMTime _lastAudio;//记录上一次音频数据文件的CMTime
}
@property (nonatomic) BOOL audioStatus;
@property (nonatomic) BOOL videoStatus;
@property (nonatomic) int fps;
@property (nonatomic, assign) CGSize size;
@property (nonatomic, assign) CGSize screenSize;
@property (nonatomic, strong) NSString *filePath;

@property (nonatomic, strong) AVAssetWriterInputPixelBufferAdaptor *adaptor;
@property (nonatomic, strong) AVAssetWriter *writer;
@property (nonatomic, strong) AVAssetWriterInput *videoWriterInput;
@property (nonatomic, strong) AVAssetWriterInput *audioWriterInput;

@property (copy  , nonatomic) dispatch_queue_t captureQueue;//录制的队列

@property (strong, nonatomic) AVCaptureSession *recordSession;//捕获视频的会话
@property (strong, nonatomic) AVCaptureConnection        *audioConnection;//音频录制连接
@property (strong, nonatomic) AVCaptureConnection        *videoConnection;//视频录制连接
@property (strong, nonatomic) AVCaptureVideoDataOutput   *videoOutput;//视频输出
@property (strong, nonatomic) AVCaptureAudioDataOutput   *audioOutput;//音频输出
@property (strong, nonatomic) AVCaptureDeviceInput *backCameraInput;//后置摄像头输入
@property (strong, nonatomic) AVCaptureDeviceInput *frontCameraInput;//前置摄像头输入
@property (strong, nonatomic) AVCaptureDeviceInput *audioMicInput;//麦克风输入

@property (nonatomic) BOOL isPaused;
@property (nonatomic) int frameCount;
@property (atomic, assign) BOOL discont;//是否中断
@property (atomic, assign) CMTime startTime;//开始录制的时间
@property (atomic, assign) CMTime diffTime; // 捕获的偏差时间
@property (atomic, assign) CGFloat currentRecordTime;//当前录制时间
@end

@implementation MediaToolsSession
-(id)init
{
    self.audioStatus = FALSE;
    self.videoStatus = FALSE;
    self.frameCount = 0;
    self.fps = 0;
    self.startTime = CMTimeMake(0, 0);
	CGRect screenRect = [[UIScreen mainScreen] bounds];
	self.screenSize = screenRect.size;
    return [super init];
}

-(CVPixelBufferRef) pixelBufferFromCGImage:(CGImageRef) image{
    
    CGSize size = CGSizeMake(CGImageGetWidth(image), CGImageGetHeight(image));
    NSDictionary *options = [NSDictionary dictionaryWithObjectsAndKeys:
                             [NSNumber numberWithBool:YES], kCVPixelBufferCGImageCompatibilityKey,
                             [NSNumber numberWithBool:YES], kCVPixelBufferCGBitmapContextCompatibilityKey,
                             nil];
    CVPixelBufferRef pxbuffer = NULL;
    CVReturn status = CVPixelBufferCreate(kCFAllocatorDefault, size.width, size.height, kCVPixelFormatType_32ARGB, (__bridge CFDictionaryRef) options, &pxbuffer);
    NSParameterAssert(status == kCVReturnSuccess && pxbuffer != NULL);
    
    CVPixelBufferLockBaseAddress(pxbuffer, 0);
    void *pxdata = CVPixelBufferGetBaseAddress(pxbuffer);
    NSParameterAssert(pxdata != NULL);
    CGColorSpaceRef rgbColorSpace = CGColorSpaceCreateDeviceRGB();
    CGContextRef context = CGBitmapContextCreate(pxdata, size.width, size.height, 8, 4*size.width, rgbColorSpace, kCGImageAlphaNoneSkipFirst);
    NSParameterAssert(context);
    CGContextConcatCTM(context, CGAffineTransformMakeRotation(0));
    CGContextDrawImage(context, CGRectMake(0, 0, size.width, size.height), image);
    CGColorSpaceRelease(rgbColorSpace);
    CGContextRelease(context);
    CVPixelBufferUnlockBaseAddress(pxbuffer, 0);
    return pxbuffer;
}

- (CMSampleBufferRef)sampleBufferFromCGImage:(CGImageRef)image withTime:(CMSampleTimingInfo)timimgInfo withVideo:(CMVideoFormatDescriptionRef)videoInfo
{
    CVPixelBufferRef pixelBuffer = [self pixelBufferFromCGImage:image];
    CMSampleBufferRef newSampleBuffer = NULL;
    CMVideoFormatDescriptionCreateForImageBuffer(
                                                 NULL, pixelBuffer, &videoInfo);
    CMSampleBufferCreateReadyWithImageBuffer(kCFAllocatorDefault,
                                             pixelBuffer,
                                             videoInfo,
                                             &timimgInfo,
                                             &newSampleBuffer);
    
    return newSampleBuffer;
}
-(bool) preAudio{
    self.audioStatus = true;
    
    return TRUE;
}
-(void) initAudio:(CMSampleBufferRef)sampleBuffer{
    CMFormatDescriptionRef fmt = CMSampleBufferGetFormatDescription(sampleBuffer);
    const AudioStreamBasicDescription *asbd = CMAudioFormatDescriptionGetStreamBasicDescription(fmt);
    NSDictionary *audioSettings = [NSDictionary dictionaryWithObjectsAndKeys:
                                   [ NSNumber numberWithInt: kAudioFormatMPEG4AAC], AVFormatIDKey,
                                   [ NSNumber numberWithInt: asbd->mSampleRate], AVSampleRateKey,
                                   [ NSNumber numberWithFloat: asbd->mChannelsPerFrame], AVNumberOfChannelsKey,
//                                   [ NSNumber numberWithInt:16],AVLinearPCMBitDepthKey,//采样位数 默认 16
//                                   [ NSNumber numberWithInt:1], AVLinearPCMIsFloatKey,
//                                   [ NSNumber numberWithInt: 128000], AVEncoderBitRateKey,
                                   nil];
    self.audioWriterInput = [AVAssetWriterInput assetWriterInputWithMediaType: AVMediaTypeAudio
                                                               outputSettings:audioSettings];
//    self.audioWriterInput.expectsMediaDataInRealTime = YES;
    [self.writer addInput:[self audioWriterInput]];
}

-(bool)preVideo:(CGSize) size withFPS:(int)fps{
    self.videoStatus = TRUE;
    self.frameCount = 0;
    self.fps = fps;
    self.size = size;
	
	//写入视频大小
	NSInteger numPixels = self.size.width * self.size.height;
	//每像素比特
	CGFloat bitsPerPixel = 8.0;
	NSInteger bitsPerSecond = numPixels * bitsPerPixel;
	// 码率和帧率设置
	NSDictionary *compressionProperties = @{ AVVideoAverageBitRateKey : @(bitsPerSecond),
                                            AVVideoExpectedSourceFrameRateKey : @(fps),
                                            AVVideoMaxKeyFrameIntervalKey : @(fps),
											AVVideoProfileLevelKey : AVVideoProfileLevelH264Main30 };
	
    NSDictionary *videoSettings = [NSDictionary dictionaryWithObjectsAndKeys:AVVideoCodecH264, AVVideoCodecKey,
                                   [NSNumber numberWithInt:self.size.width], AVVideoWidthKey,
                                   [NSNumber numberWithInt:self.size.height], AVVideoHeightKey,
                                   [NSDictionary dictionaryWithDictionary:compressionProperties], AVVideoCompressionPropertiesKey,
                                   nil];
    
    self.videoWriterInput = [AVAssetWriterInput assetWriterInputWithMediaType: AVMediaTypeVideo
                                                               outputSettings:videoSettings];
    self.videoWriterInput.expectsMediaDataInRealTime = YES;
    self.adaptor = [AVAssetWriterInputPixelBufferAdaptor assetWriterInputPixelBufferAdaptorWithAssetWriterInput: self.videoWriterInput sourcePixelBufferAttributes:nil];
    return TRUE;
}
-(BOOL) sampleVideo:(GLubyte *)_buffer withSize:(CGSize) size
{
    if(![self videoStatus]){
        return FALSE;
    }
    if([self isPaused]){
        return TRUE;
    }
    // 如果有麦克风，等待音频
    if(self.audioStatus && self.audioWriterInput == nil){
        return TRUE;
    }
//    CMSampleTimingInfo* t = (CMSampleTimingInfo*)malloc(sizeof(CMSampleTimingInfo));
//    t->duration = CMTimeMake(1, self.fps);
//    t->presentationTimeStamp = CMTimeMake(self.frameCount, self.fps);
//    t->decodeTimeStamp = CMTimeMake(self.frameCount, self.fps);
//    CMVideoFormatDescriptionRef v;
//    CMVideoFormatDescriptionCreate(kCFAllocatorDefault, kCMVideoCodecType_H264, self.size.width, self.size.height, NULL, &v);
//    CMSampleBufferRef sampleBuffer = [self sampleBufferFromCGImage:[image CGImage] withTime:*t withVideo:v];
//
//    [self preProcessSample:sampleBuffer isVideo:TRUE];
    
//    UIGraphicsBeginImageContext(self.size); // this will crop
//    [image drawInRect:CGRectMake(0,0,self.size.width,self.size.height)];
//    UIImage* newImage = UIGraphicsGetImageFromCurrentImageContext();
//    UIGraphicsEndImageContext();
    
//    CVPixelBufferRef buffer = [self pixelBufferFromCGImage:[image CGImage]];
    CVPixelBufferRef buffer = NULL;
    CVReturn ret = CVPixelBufferCreateWithBytes(kCFAllocatorDefault,size.width,size.height,kCVPixelFormatType_32BGRA,_buffer,size.width*4,NULL,NULL,NULL,&buffer);
    if(ret != kCVReturnSuccess){
        NSLog(@"buffer is error:%i", ret);
        return NO;
    }
    @synchronized(self) {
        self.frameCount += 1;
        if(self.startTime.value == 0){
            free(_buffer);
            CVPixelBufferRelease(buffer);
            return TRUE;
        }
        CMTime frameTime = CMTimeMake((int64_t)([[NSDate date] timeIntervalSince1970]  * 1000), 1000);
        frameTime = CMTimeAdd(self.diffTime, frameTime);
//        NSLog(@"video:%lld", frameTime.value);
        [self preProcessTime:frameTime duration:kCMTimeZero isVideo:true];
        dispatch_async(_captureQueue, ^{
            if(![self writeInit:true]){
                NSLog(@"sampleVideo:writeinit:false");
                return;
            }
            //视频输入是否准备接受更多的媒体数据
            if (self.videoWriterInput.readyForMoreMediaData != YES) {
                NSLog(@"sampleVideo:ready:false");
                return;
            }
            BOOL append = [self.adaptor appendPixelBuffer:buffer withPresentationTime:frameTime];
            if(!append){
                NSLog(@"sampleVideo:append:false");
                return;
            }
            free(_buffer);
            CVPixelBufferRelease(buffer);
        });
    }
    return TRUE;
}
-(bool)startRecord:(NSString *)file{
    if(!self.audioStatus && !self.videoStatus){
        return FALSE;
    }
    self.isPaused = NO;
    self.discont = NO;
    self.filePath = file;
    NSError *err = nil;
    self.recordSession = [self recordSessionInit];
    self.writer = [[AVAssetWriter alloc] initWithURL:[NSURL fileURLWithPath:file] fileType:AVFileTypeQuickTimeMovie error:&err];
    if(![self writer]){
        return FALSE;
    }
    // 在录像时添加
//    if(self.audioStatus && ![self.writer canAddInput:[self audioWriterInput]]){
//        return FALSE;
//    }
    if(self.videoStatus){
        if(![self.writer canAddInput:[self videoWriterInput]]) return FALSE;
        [self.writer addInput:[self videoWriterInput]];
    }
    self.writer.shouldOptimizeForNetworkUse = TRUE;
    [self.recordSession startRunning];
    return TRUE;
}

-(void)stopRecord{
    [self.recordSession stopRunning];
    @synchronized(self) {
        dispatch_async(_captureQueue, ^{
            NSArray<AVAssetWriterInput *> *inputs = self.writer.inputs;
            for (AVAssetWriterInput *writerInput in inputs) {
                [writerInput markAsFinished];
            }
//            dispatch_async(dispatch_get_main_queue(), ^{
//                [self recordProgress:self.currentRecordTime];
//            });
            [self.writer finishWritingWithCompletionHandler:^(void){
                self.startTime = CMTimeMake(0, 0);
                self.currentRecordTime = 0;
//                if (UIVideoAtPathIsCompatibleWithSavedPhotosAlbum (self.filePath)) {
//                    UISaveVideoAtPathToSavedPhotosAlbum (self.filePath, nil, nil, nil);
//                }else{
//                    NSLog(@"无法保存");
//                }
                if(self->_finish_callback) self->_finish_callback(true);
            }];
            self.videoStatus = FALSE;
            self.audioStatus = FALSE;
            self.isPaused = FALSE;
        });
    }
    
}
-(void)pauseRecord{
    self.isPaused = YES;
    self.discont = YES;
}

-(void)resumeRecord{
    self.isPaused = false;
}

-(void)recordProgress:(CGFloat) time{
    if(_process_callback) _process_callback(time);
}

- (void)releaseRecord
{
    self.adaptor = nil;
    self.writer = nil;
    self.videoWriterInput = nil;
    self.audioWriterInput = nil;
    
    _captureQueue     = nil;
    _audioOutput      = nil;
    _videoOutput      = nil;
	_recordSession    = nil;
}

#pragma mark - 写入数据
- (void) captureOutput:(AVCaptureOutput *)captureOutput didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer fromConnection:(AVCaptureConnection *)connection {
    BOOL isVideo = YES;
    BOOL flag = false;
    if (!(self.audioStatus || self.videoStatus)  || self.isPaused) {
        flag = true;
        return;
    }
    if (captureOutput == self.audioOutput) {
        isVideo = NO;
    }
    CMTime pts = CMSampleBufferGetPresentationTimeStamp(sampleBuffer);
    CMTime dur = CMSampleBufferGetDuration(sampleBuffer);
    @synchronized(self) {
        //初始化编码器，当有音频和视频参数时创建编码器
        if(self.audioStatus && self.audioWriterInput == nil && !isVideo){
            [self initAudio:sampleBuffer];
        }
        
        // 初始化时间，确保视频已经开始捕获
        if (self.startTime.value == 0 && self.frameCount > 0) {
            self.startTime = pts;
            self.diffTime = CMTimeSubtract(self.startTime, CMTimeMake((int64_t)([[NSDate date] timeIntervalSince1970]  * 1000), 1000));
        }
    }
//    NSLog(@"audio:%lld", pts.value);
    CMTime offset = [self preProcessTime:pts duration:dur isVideo:isVideo];
//    NSLog(@"pts:%lld", pts.value);
    if (offset.value > 0) {
        //根据得到的timeOffset调整
        sampleBuffer = [self adjustTime:sampleBuffer by:_timeOffset];
    }
    // 进行数据编码
    [self encodeFrame:sampleBuffer isVideo:isVideo];
    if(offset.value > 0){
        CFRelease(sampleBuffer);
    }
}
- (CMTime)preProcessTime:(CMTime) pts duration:(CMTime) dur isVideo:(BOOL)isVideo{
    @synchronized(self) {
        //判断是否中断录制过
        if (self.discont) {
            self.discont = NO;
            // 计算暂停的时间
            CMTime last = isVideo ? _lastVideo : _lastAudio;
            if (last.flags & kCMTimeFlags_Valid) {
                if (_timeOffset.flags & kCMTimeFlags_Valid) {
                    pts = CMTimeSubtract(pts, _timeOffset);
                }
                CMTime offset = CMTimeSubtract(pts, last);
                if (_timeOffset.value == 0) {
                    _timeOffset = offset;
                }else {
                    _timeOffset = CMTimeAdd(_timeOffset, offset);
                }
            }
            _lastVideo.flags = 0;
            _lastAudio.flags = 0;
        }
        // 记录暂停上一次录制的时间
        pts = CMTimeSubtract(pts, _timeOffset);
        if (dur.value > 0) {
            pts = CMTimeAdd(pts, dur);
        }
        if (isVideo) {
            _lastVideo = pts;
        }else {
            _lastAudio = pts;
        }
        CMTime sub = CMTimeSubtract(pts, self.startTime);
        self.currentRecordTime = CMTimeGetSeconds(sub);
        dispatch_async(dispatch_get_main_queue(), ^{
            [self recordProgress:self.currentRecordTime];
        });
        return _timeOffset;
    }
}
//通过这个方法写入数据
- (BOOL)encodeFrame:(CMSampleBufferRef) sampleBuffer isVideo:(BOOL)isVideo {
    //数据是否准备写入
    if (CMSampleBufferDataIsReady(sampleBuffer)) {
        if(![self writeInit:isVideo]){
            return NO;
        }
        if(!isVideo && self.frameCount == 0) {
            // 等待视频第一帧
            return YES;
        }
        
        //判断是否是视频
        if (isVideo) {
            //视频输入是否准备接受更多的媒体数据
            if (self.videoWriterInput.readyForMoreMediaData == YES) {
                //拼接数据
                [self.videoWriterInput appendSampleBuffer:sampleBuffer];
                return YES;
            }
        }else {
            //音频输入是否准备接受更多的媒体数据
            if (self.audioWriterInput.readyForMoreMediaData) {
                //拼接数据
                [self.audioWriterInput appendSampleBuffer:sampleBuffer];
                return YES;
            }
        }
    }
    return NO;
}
-(BOOL) writeInit:(BOOL)isVideo{
    //写入状态为未知,保证视频先写入
    if (self.writer.status == AVAssetWriterStatusUnknown && isVideo) {
        //开始写入
        [self.writer startWriting];
        NSLog(@"write:%lld", self.startTime.value);
        [self.writer startSessionAtSourceTime:self.startTime];
    }
    //写入失败
    if (self.writer.status == AVAssetWriterStatusFailed) {
        NSLog(@"writer error %@", self.writer.error);
        return NO;
    }
    return YES;
}

#pragma -mark 将mov文件转为MP4文件
- (void)changeMovToMp4:(NSString *)mediaFile with:(NSString *)sourceFile dataBlock:(void (^)(void))handler {
    NSURL* sourceUrl = [NSURL fileURLWithPath:sourceFile];
    AVAsset *source = [AVAsset assetWithURL:sourceUrl];
    AVAssetExportSession *exportSession = [AVAssetExportSession exportSessionWithAsset:source presetName:AVAssetExportPreset1280x720];
    exportSession.shouldOptimizeForNetworkUse = YES;
    exportSession.outputFileType = AVFileTypeMPEG4;
    exportSession.outputURL = [NSURL fileURLWithPath:mediaFile];
    [exportSession exportAsynchronouslyWithCompletionHandler:handler];
}
//获取视频第一帧的图片
+ (void)movieToImage:(void (^)(UIImage *movieImage))handler with:(NSString *)sourceFile {
    NSURL *url = [NSURL fileURLWithPath:sourceFile];
    AVURLAsset *asset = [[AVURLAsset alloc] initWithURL:url options:nil];
    AVAssetImageGenerator *generator = [[AVAssetImageGenerator alloc] initWithAsset:asset];
    generator.appliesPreferredTrackTransform = TRUE;
    CMTime thumbTime = CMTimeMakeWithSeconds(0, 60);
    generator.apertureMode = AVAssetImageGeneratorApertureModeEncodedPixels;
    AVAssetImageGeneratorCompletionHandler generatorHandler =
    ^(CMTime requestedTime, CGImageRef im, CMTime actualTime, AVAssetImageGeneratorResult result, NSError *error){
        if (result == AVAssetImageGeneratorSucceeded) {
            UIImage *thumbImg = [UIImage imageWithCGImage:im];
            if (handler) {
                dispatch_async(dispatch_get_main_queue(), ^{
                    handler(thumbImg);
                });
            }
        }
    };
    [generator generateCGImagesAsynchronouslyForTimes:
    [NSArray arrayWithObject:[NSValue valueWithCMTime:thumbTime]] completionHandler:generatorHandler];
}
#pragma mark - 捕获session处理
//捕获视频的会话
- (AVCaptureSession *)recordSessionInit {
    AVCaptureSession *session = [[AVCaptureSession alloc] init];
    //添加后置摄像头的输入
//    if ([session canAddInput:self.backCameraInput]) {
//        [session addInput:self.backCameraInput];
//    }
    //添加后置麦克风的输出
    if ([session canAddInput:self.audioMicInput]) {
        [session addInput:self.audioMicInput];
    }
    //添加视频输出
//    if ([session canAddOutput:self.videoOutput]) {
//        [session addOutput:self.videoOutput];
//    }
    //添加音频输出
    if ([session canAddOutput:self.audioOutput]) {
        [session addOutput:self.audioOutput];
    }
    //设置视频录制的方向
    //    self.videoConnection.videoOrientation = AVCaptureVideoOrientationPortrait;
    return session;
}
#pragma -mark 捕获输出
//录制的队列
- (dispatch_queue_t)captureQueue {
    if (_captureQueue == nil) {
        _captureQueue = dispatch_queue_create("com.nuliji.media_tools.capture", DISPATCH_QUEUE_SERIAL);
    }
    return _captureQueue;
}
//视频输出
- (AVCaptureVideoDataOutput *)videoOutput {
    if (_videoOutput == nil) {
        _videoOutput = [[AVCaptureVideoDataOutput alloc] init];
        [_videoOutput setSampleBufferDelegate:self queue:self.captureQueue];
        NSDictionary* setcapSettings = [NSDictionary dictionaryWithObjectsAndKeys:
                                        [NSNumber numberWithInt:kCVPixelFormatType_420YpCbCr8BiPlanarVideoRange], kCVPixelBufferPixelFormatTypeKey,
                                        nil];
        _videoOutput.videoSettings = setcapSettings;
    }
    return _videoOutput;
}

//音频输出
- (AVCaptureAudioDataOutput *)audioOutput {
    if (_audioOutput == nil) {
        _audioOutput = [[AVCaptureAudioDataOutput alloc] init];
        [_audioOutput setSampleBufferDelegate:self queue:self.captureQueue];
    }
    return _audioOutput;
}
#pragma -mark 捕获输入
//后置摄像头输入
- (AVCaptureDeviceInput *)backCameraInput {
    if (_backCameraInput == nil) {
        NSError *error;
        _backCameraInput = [[AVCaptureDeviceInput alloc] initWithDevice:[self backCamera] error:&error];
        if (error) {
            NSLog(@"获取后置摄像头失败~");
        }
    }
    return _backCameraInput;
}
//前置摄像头输入
- (AVCaptureDeviceInput *)frontCameraInput {
    if (_frontCameraInput == nil) {
        NSError *error;
        _frontCameraInput = [[AVCaptureDeviceInput alloc] initWithDevice:[self frontCamera] error:&error];
        if (error) {
            NSLog(@"获取前置摄像头失败~");
        }
    }
    return _frontCameraInput;
}
//麦克风输入
- (AVCaptureDeviceInput *)audioMicInput {
    if (_audioMicInput == nil) {
        AVCaptureDevice *mic = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeAudio];
        NSError *error;
        _audioMicInput = [AVCaptureDeviceInput deviceInputWithDevice:mic error:&error];
        if (error) {
            NSLog(@"获取麦克风失败~");
        }
    }
    return _audioMicInput;
}
#pragma mark - 视频相关
//返回前置摄像头
- (AVCaptureDevice *)frontCamera {
    return [self cameraWithPosition:AVCaptureDevicePositionFront];
}

//返回后置摄像头
- (AVCaptureDevice *)backCamera {
    return [self cameraWithPosition:AVCaptureDevicePositionBack];
}
//用来返回是前置摄像头还是后置摄像头
- (AVCaptureDevice *)cameraWithPosition:(AVCaptureDevicePosition) position {
    //返回和视频录制相关的所有默认设备
    NSArray *devices = [AVCaptureDevice devicesWithMediaType:AVMediaTypeVideo];
    //遍历这些设备返回跟position相关的设备
    for (AVCaptureDevice *device in devices) {
        if ([device position] == position) {
            return device;
        }
    }
    return nil;
}

//开启闪光灯
- (void)openFlashLight {
    AVCaptureDevice *backCamera = [self backCamera];
    if (backCamera.torchMode == AVCaptureTorchModeOff) {
        [backCamera lockForConfiguration:nil];
        backCamera.torchMode = AVCaptureTorchModeOn;
        backCamera.flashMode = AVCaptureFlashModeOn;
        [backCamera unlockForConfiguration];
    }
}
//关闭闪光灯
- (void)closeFlashLight {
    AVCaptureDevice *backCamera = [self backCamera];
    if (backCamera.torchMode == AVCaptureTorchModeOn) {
        [backCamera lockForConfiguration:nil];
        backCamera.torchMode = AVCaptureTorchModeOff;
        backCamera.flashMode = AVCaptureFlashModeOff;
        [backCamera unlockForConfiguration];
    }
}
//调整媒体数据的时间
- (CMSampleBufferRef)adjustTime:(CMSampleBufferRef)sample by:(CMTime)offset {
    CMItemCount count;
    CMSampleBufferGetSampleTimingInfoArray(sample, 0, nil, &count);
    CMSampleTimingInfo* pInfo = (CMSampleTimingInfo*)malloc(sizeof(CMSampleTimingInfo) * count);
    CMSampleBufferGetSampleTimingInfoArray(sample, count, pInfo, &count);
    for (CMItemCount i = 0; i < count; i++) {
        pInfo[i].decodeTimeStamp = CMTimeSubtract(pInfo[i].decodeTimeStamp, offset);
        pInfo[i].presentationTimeStamp = CMTimeSubtract(pInfo[i].presentationTimeStamp, offset);
    }
    CMSampleBufferRef sout;
    CMSampleBufferCreateCopyWithNewTiming(nil, sample, count, pInfo, &sout);
    free(pInfo);
    return sout;
}
//+(void)asdf:(CGSize) size{
//    if ([GPUImageOpenGLESContext supportsFastTextureUpload])
//    {
//        CVReturn err = CVOpenGLESTextureCacheCreate(kCFAllocatorDefault, NULL, (__bridge void *)[[GPUImageOpenGLESContext sharedImageProcessingOpenGLESContext] context], NULL, &coreVideoTextureCache);
//        if (err)
//        {
//            NSAssert(NO, @"Error at CVOpenGLESTextureCacheCreate %d");
//        }
//
//        CVPixelBufferPoolCreatePixelBuffer (NULL, [assetWriterPixelBufferInput pixelBufferPool], &renderTarget);
//
//        CVOpenGLESTextureRef renderTexture;
//        CVOpenGLESTextureCacheCreateTextureFromImage (kCFAllocatorDefault, coreVideoTextureCache, renderTarget,
//                                                      NULL,//texture attributes
//                                                      GL_TEXTURE_2D,
//                                                      GL_RGBA,//opengl format
//                                                      (int)videoSize.width,
//                                                      (int)videoSize.height,
//                                                      GL_BGRA,//native iOS format
//                                                      GL_UNSIGNED_BYTE,
//                                                      0,
//                                                      &renderTexture);
//
//        glBindTexture(CVOpenGLESTextureGetTarget(renderTexture), CVOpenGLESTextureGetName(renderTexture));
//        glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
//        glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
//
//        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, CVOpenGLESTextureGetName(renderTexture), 0);
//    }
//    CVPixelBufferLockBaseAddress(pixel_buffer, 0);
//}
- (GLubyte *) glToUIImage:(CGSize) position withSize:(CGSize) size{
    int currentFBORead;
    int currentFBOWrite;
    glGetIntegerv(GL_READ_FRAMEBUFFER_BINDING, &currentFBORead);
    glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &currentFBOWrite);
    // Set the read frame buffer
    glBindFramebuffer(GL_READ_FRAMEBUFFER, currentFBOWrite);
    GLenum status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
    if (status != GL_FRAMEBUFFER_COMPLETE)
        NSLog(@"Bind framebuffer error! : %x", status);
    
    int width = size.width;
    int height = size.height;
    
    int myDataLength = width * height * 4;
//    int bytesPerRow = 4 * width;
    
    GLubyte *buffer = (GLubyte *) malloc(myDataLength);
//    GLubyte *buffer2 = (GLubyte *) malloc(myDataLength);
    glReadPixels(position.width, position.height, size.width, size.height, GL_RGBA, GL_UNSIGNED_BYTE, buffer);
    return buffer;
//    glBindFramebuffer(GL_READ_FRAMEBUFFER, currentFBORead);
//    for(int y1 = 0; y1 < height; y1++) {
//        for(int x1 = 0; x1 <width * 4; x1++) {
//            buffer2[(height - 1 - y1) * width * 4 + x1] = buffer[y1 * 4 * width + x1];
//        }
//    }
//    free(buffer);
//
//    CGDataProviderRef provider = CGDataProviderCreateWithData(NULL, buffer2, myDataLength, NULL);
//    int bitsPerComponent = 8;
//    int bitsPerPixel = 32;
//    CGColorSpaceRef colorSpaceRef = CGColorSpaceCreateDeviceRGB();
//    CGBitmapInfo bitmapInfo = kCGBitmapByteOrderDefault;
//    CGColorRenderingIntent renderingIntent = kCGRenderingIntentDefault;
//    CGImageRef imageRef = CGImageCreate(width, height, bitsPerComponent, bitsPerPixel, bytesPerRow, colorSpaceRef, bitmapInfo, provider, NULL, NO, renderingIntent);
//    CGColorSpaceRelease(colorSpaceRef);
//    CGDataProviderRelease(provider);
//    UIImage *image = [ UIImage imageWithCGImage:imageRef scale:1 orientation:UIImageOrientationUp ];
//    NSLog(@"GL Image size: %d, %d", (int)[image size].width, (int)[image size].height);
//    return image;
//
}
-(GLubyte *)mlToImage:(CGSize) position withSize:(CGSize) size{
    UnityDisplaySurfaceMTL *surface = (UnityDisplaySurfaceMTL*)GetMainDisplaySurface();
    int width = size.width;
    int height = size.height;
    int myDataLength = width * height * 4;
    int bytesPerRow = 4 * width;
    GLubyte *buffer = (GLubyte *) malloc(myDataLength);
    
//    MTLRegion region = MTLRegionMake2D(position.width, position.height, size.width, size.height);
    MTLRegion region = MTLRegionMake2D(position.width, position.height, width, height);
//    @synchronized(surface->drawable) {
//        MTLTextureRef texture = (surface->drawable).texture;
//        [texture getBytes:buffer bytesPerRow:bytesPerRow fromRegion:region mipmapLevel:0];
//    }
    MTLTextureRef texture = (surface->drawable).texture;
    [texture getBytes:buffer bytesPerRow:bytesPerRow fromRegion:region mipmapLevel:0];
    return buffer;
//    CGDataProviderRef provider = CGDataProviderCreateWithData(NULL, buffer, myDataLength, NULL);
//    int bitsPerComponent = 8;
//    int bitsPerPixel = 32;
//    CGColorSpaceRef colorSpaceRef = CGColorSpaceCreateDeviceRGB();
//    CGBitmapInfo bitmapInfo = kCGBitmapByteOrderDefault;
//    CGColorRenderingIntent renderingIntent = kCGRenderingIntentDefault;
//    CGImageRef imageRef = CGImageCreate(width, height, bitsPerComponent, bitsPerPixel, bytesPerRow, colorSpaceRef, bitmapInfo, provider, NULL, NO, renderingIntent);
//    CGColorSpaceRelease(colorSpaceRef);
//    CGDataProviderRelease(provider);
//    UIImage *image = [ UIImage imageWithCGImage:imageRef scale:1 orientation:UIImageOrientationUp ];
//    image = [image imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal];
//    NSLog(@"ML Image size: %d, %d", (int)[image size].width, (int)[image size].height);
//    return image;
}
-(UIImage *)captureImage
{
    UIView * view = [[UIScreen mainScreen] snapshotViewAfterScreenUpdates:YES];
    if(UIGraphicsBeginImageContextWithOptions != NULL)
    {
        UIGraphicsBeginImageContextWithOptions(view.frame.size, NO, 0.0);
    } else {
        UIGraphicsBeginImageContext(view.frame.size);
    }
    [view.layer renderInContext:UIGraphicsGetCurrentContext()];
    UIImage *image = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    return image;
}
@end

extern "C" {
    void* unity_createRecordSession(){
        MediaToolsSession *mediaToolsSession = [[MediaToolsSession alloc] init];
        return (__bridge_retained void*)mediaToolsSession;
    }
    bool unity_preAudio(void* mediaToolsSession, UNITY_AUDIO_ENCODE_ERROR_CALLBACK error_callback) {
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        return [session preAudio];
    }
    
    bool unity_preVideo(void* mediaToolsSession, int width, int height, int fps, UNITY_VIDEO_ENCODE_ERROR_CALLBACK error_callback){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        CGSize size = CGSizeMake(width, height);
        return [session preVideo:size withFPS:fps];
    }
    
    void unity_processRecord(void* mediaToolsSession, UNITY_RECORD_PROCESS_CALLBACK process_callback){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        session->_process_callback = process_callback;
    }
    
    bool unity_startRecord(void* mediaToolsSession, const char *file){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        return [session startRecord:[NSString stringWithUTF8String:file]];
    }
    
    void unity_pauseRecord(void* mediaToolsSession){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        return [session pauseRecord];
    }
    void unity_resumeRecord(void* mediaToolsSession){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        return [session resumeRecord];
    }
    
    void unity_stopRecord(void* mediaToolsSession, UNITY_RECORD_FINISH_CALLBACK finish_callback){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        session->_finish_callback = finish_callback;
        return [session stopRecord];
    }
    
    bool unity_sampleVideo(void* mediaToolsSession, int x, int y, int width, int height){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
//        NSData *data = [NSData dataWithBytes:(const void *)buffer length:(sizeof(unsigned char) * length)];
//        UIImage *image = [UIImage imageWithData:data];
        CGSize position = CGSizeMake(x, y);
        CGSize size = CGSizeMake(width, height);
        GLubyte *buffer;
        if(GetAppController().renderingAPI == apiMetal)
            buffer = [session mlToImage:position withSize:size];
        else
            buffer = [session glToUIImage:position withSize:size];
        return [session sampleVideo:buffer withSize:size];
    }
    
    void unity_releaseRecordSession(void* mediaToolsSession){
        MediaToolsSession* session = (__bridge MediaToolsSession*)mediaToolsSession;
        return [session releaseRecord];
    }
    
    void unity_movieToImage(const char *file, const char *file1, UNITY_MEDIA_IMAGE_CALLBACK callback){
        NSString* img = [NSString stringWithUTF8String:file1];
        [MediaToolsSession movieToImage:^(UIImage *image){
            NSData *data = UIImagePNGRepresentation(image);
            // todo
            [data writeToFile:img atomically:YES];
            callback();
        } with:[NSString stringWithUTF8String:file]];
    }
    
    void unity_bindView(){
        HideSplashScreen();
        UnityAppController* controller = GetAppController();
        UIWindow* _window = controller.window;

        [_window addSubview: controller.rootView];
        _window.rootViewController = controller.rootViewController;
        [_window bringSubviewToFront: controller.rootView];
        
    }
}

