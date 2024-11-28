using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Interface
{
    public interface ISDKAgent
    {
        /// <summary>
        /// 第三方登录
        /// </summary>
        public bool Login();
        /// <summary>
        /// 判断当前是否已登录
        /// </summary>
        /// <returns></returns>
        public bool IsLogin();

        public string ErrorsDesc { get; set; }

        #region TOKENS
        public string Token { get; set; }
        public long TokenExpireAt { get; set; }
        public string AccessToken { get; set; }
        public dynamic FullToken { get; set; }
        public dynamic AccountInfo { get; }
        #endregion


        #region DEVICE INFO
        public string Platform { get; }
        public string AppTag { get; }

        public dynamic DeviceInfo();
        public string CountryCode { get; }
        #endregion

    }
}
