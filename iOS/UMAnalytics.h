//
//  UMAnalytics.h
//  UmengGame
//
//  Created by go on 17-10-26.
//
//

#ifndef __UmengGame__UMAnalytics__
#define __UmengGame__UMAnalytics__


extern "C"
{
    void analyticsEvent(const char* event);
    void analyticsPayCashForCoin(double cash, int source, double coin);
    void analyticsPayCashForItem(double cash, int source, const char* item, int amount, double price);
    void analyticsBuy(const char* item, int amount, double price);

}
#endif /* defined(__UmengGame__UMAnalytics__) */
