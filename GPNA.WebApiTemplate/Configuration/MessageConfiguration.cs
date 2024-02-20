namespace GPNA.gRPCClient.Configuration
{
    #region Using
    using System;
    #endregion Using

    /// <summary>
    /// Конфигурация передаваемого сообщения
    /// </summary>
    public class MessageConfiguration
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Год
        /// </summary>
        public int  Value { get; set; } = 0;
    }
}
