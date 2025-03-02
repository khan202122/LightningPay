﻿using System;

namespace LightningPay
{
    /// <summary>
    ///  Exception thrown by LightningPay
    /// </summary>
    public class LightningPayException : Exception
    {
        /// <summary>Gets or sets the http status code.</summary>
        /// <value>The http status code.</value>
        public ErrorCode Code { get; set; }

        /// <summary>Gets or sets the response data.</summary>
        /// <value>The response data.</value>
        public string ResponseData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightningPayException"/> class.
        /// </summary>
        public LightningPayException()
        { }


        /// <summary>
        /// Initializes a new instance of the <see cref="LightningPayException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LightningPayException(string message)
            : base(message)
        { }


        /// <summary>Initializes a new instance of the <see cref="LightningPayException" /> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="code">The code.</param>
        /// <param name="responseData">The response data.</param>
        /// <param name="innerException">The inner exception.</param>
        public LightningPayException(string message,
            ErrorCode code,
            string responseData = null,
            Exception innerException = null)
            : base(message, innerException)
        {
            this.Code = code;
            this.ResponseData = responseData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightningPayException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public LightningPayException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        ///   LightningPay error code
        /// </summary>
        public enum ErrorCode
        {
            /// <summary>Bad configuration</summary>
            BAD_CONFIGURATION,
            /// <summary>Unauthorized Access</summary>
            UNAUTHORIZED,
            /// <summary>Bad request</summary>
            BAD_REQUEST,
            /// <summary>Internal error</summary>
            INTERNAL_ERROR
        }
    }
}
