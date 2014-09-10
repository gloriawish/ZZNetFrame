using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netframe.Model
{
    public enum Commands:long
    {
        None = 0,//无操作
        Entry = 1,//进入
        Exit = 2,//退出
        Absence = 4,//离开
        SendMsg = 5,//发送消息
        RecvConfirm = 6,//接受
        ReadMsg = 7,//读消息
        DelMsg = 8,//删除消息
        GetInfo = 9,//获取版本信息
        SendInfo = 10//发送版本信息
    }
}
