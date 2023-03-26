namespace GPNA.WebApiSender.Model
{
    #region Using
    using System;
    #endregion Using

    /// <summary>
    /// Регистр отчетов
    /// </summary>
    public class SampleReport
    {
        /// <summary>
        /// Наименование параметра
        /// </summary>
        public string? Name { get; set; } = "abc";

        /// <summary>
        /// Значение
        /// </summary>
        public int? Value { get; set; } = 0;
    }
}
