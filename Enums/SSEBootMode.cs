using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Enums
{
    /// <summary>
    /// 最强搜索叽核心启动方式
    /// </summary>
    public enum SSEBootMode
    {
        /// <summary>
        /// 桌面版
        /// </summary>
        FOR_RELEASE = 1,
        /// <summary>
        /// 登录器
        /// </summary>
        FOR_DELEGATE =2,
        /// <summary>
        /// 服务器使用
        /// </summary>
        FOR_SERVER =3,
    }
}
