using System.ComponentModel;

namespace RS.Tools.Common.Enums
{
    /// <summary>
    /// 指令执行类型
    /// </summary>
    [DefaultValue(Method)]
    public enum InvokeType
    {
        /// <summary>
        /// Method
        /// </summary>
        Method,

        /// <summary>
        /// Action
        /// </summary>
        Action
    }
}
