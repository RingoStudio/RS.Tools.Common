using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Enums
{
    public enum WechatMessageType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        Text = 1,
        /// <summary>
        /// 图片消息
        /// </summary>
        Image = 3,
        /// <summary>
        /// 语音消息
        /// </summary>
        Voice = 34,
        /// <summary>
        /// 好友确认
        /// </summary>
        FriendVerify = 37,
        /// <summary>
        /// 
        /// </summary>
        PossibleFriend = 40,
        /// <summary>
        /// 共享名片
        /// </summary>
        Card = 42,
        /// <summary>
        /// 视频消息
        /// </summary>
        Video = 43,
        /// <summary>
        /// 动画表情
        /// </summary>
        Emoij = 47,
        /// <summary>
        /// 定位
        /// </summary>
        Location = 48,
        /// <summary>
        /// 合并转发聊天记录/引用/转发任何卡片内容
        /// </summary>
        File = 49,
        /// <summary>
        /// 微信初始化消息
        /// </summary>
        WechatInitialize = 51,
        /// <summary>
        /// 语音/视频通话通知
        /// </summary>
        VoipNotify = 52,
        /// <summary>
        /// 语音/视频通话邀请
        /// </summary>
        VopiInvite = 53,
        /// <summary>
        /// 小视频
        /// </summary>
        MiniVideo = 62,
        /// <summary>
        /// 系统提示
        /// </summary>
        SystemNotice = 9999,
        /// <summary>
        /// 撤回
        /// </summary>
        Recall = 10000,
        /// <summary>
        /// 系统消息
        /// </summary>
        SystemInfo = 10002,
    }
}
