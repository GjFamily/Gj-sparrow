//
//  UMAnalytics.mm
//  UmengGame
//
//  Created by go on 17-10-26.
//
//
#import "UMAnalytics.h"
#import <UMAnalytics/MobClick.h>
#import <UMAnalytics/MobClickGameAnalytics.h>

void analyticsEvent(const char* event){
    NSString* eventId = [NSString stringWithUTF8String:event];
    [MobClick event:eventId];
}

void analyticsPayCashForCoin(double cash, int source, double coin){
    [MobClickGameAnalytics pay:cash source:source coin:coin];
}
void analyticsPayCashForItem(double cash, int source, const char* item, int amount, double price){
    [MobClickGameAnalytics pay:cash source:source item:[NSString stringWithUTF8String:item] amount:amount price:price];
}
void analyticsBuy(const char* item, int amount, double price){
    [MobClickGameAnalytics buy:[NSString stringWithUTF8String:item] amount:amount price:price];
}
