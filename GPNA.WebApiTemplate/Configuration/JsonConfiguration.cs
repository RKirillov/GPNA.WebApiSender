﻿namespace GPNA.WebApiSender.Configuration
{
    #region Using
    using System;
    #endregion Using

    /// <summary>
    /// Конфигурация передаваемого сообщения
    /// </summary>
    public class JsonConfiguration
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Год
        /// </summary>
        public int Value { get; set; } = 0;
    }
}
