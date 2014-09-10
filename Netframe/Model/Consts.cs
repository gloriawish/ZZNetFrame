using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netframe.Model
{
    public enum Consts:long
    {
        MAX_UDP_PACKAGE_LENGTH=1024,
        //文本消息
        MESSAGE_TEXT=0,
        //二进制消息
        MESSAGE_BINARY=1
    }
}
