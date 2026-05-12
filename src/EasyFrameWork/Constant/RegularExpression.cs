/* http://www.zkea.net/ 
 * Copyright (c) ZKEASOFT. All rights reserved. 
 * http://www.zkea.net/licenses */

namespace Easy.Constant
{
    public class RegularExpression
    {
        /// <summary>
        /// Email
        /// </summary>
        public const string Email = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        /// <summary>
        /// URL
        /// </summary>
        public const string Url = @"[a-zA-Z]+://[^\s]*";
        /// <summary>
        /// Chinese characters
        /// </summary>
        public const string Chinese = @"[\u4e00-\u9fa5]";
        /// <summary>
        /// HTML
        /// </summary>
        public const string Html = @"<(\S*?)[^>]*>.*?</\1>|<.*? />";
        /// <summary>
        /// Username
        /// </summary>
        public const string UserName = @"^[a-zA-Z][a-zA-Z0-9_]{4,15}$";
        /// <summary>
        /// Landline phone
        /// </summary>
        public const string ChinesePhone = @"\d{3}-\d{8}|\d{4}-\d{7}";
        /// <summary>
        /// Mobile phone number
        /// </summary>
        public const string ChineseMobile = @"^1[34578]\d{9}$";
        /// <summary>
        /// Zip code
        /// </summary>
        public const string ZipCode = @"[0-9]\d{5}(?!\d)";
        /// <summary>
        /// ID card
        /// </summary>
        public const string CardID = @"\d{15}|\d{18}";
        /// <summary>
        /// IP address
        /// </summary>
        public const string IpAddress = @"\d+\.\d+\.\d+\.\d+";
        /// <summary>
        /// Match positive integers
        /// </summary>
        public const string PositiveIntegers = @"^[1-9]\d*$";
        /// <summary>
        /// Match negative integers
        /// </summary>
        public const string NegativeIntegers = @"^-[1-9]\d*$";
        /// <summary>
        /// Match integers
        /// </summary>
        public const string Integer = @"^-?[0-9]+$";
        /// <summary>
        /// Match non-negative integers (positive integers + 0)
        /// </summary>
        public const string PositiveIntegersAndZero = @"^[0-9]+$";
        /// <summary>
        /// Match non-positive integers (negative integers + 0)
        /// </summary>
        public const string NegativeIntegersAndZero = @"^-[0-9]+$";
        /// <summary>
        /// Match positive floating numbers
        /// </summary>
        public const string Float = @"^(\-|\+)?\d+(\.\d+)?$";
        /// <summary>
        /// Match strings composed of 26 English letters
        /// </summary>
        public const string Letters = @"^[A-Za-z]+$";
        /// <summary>
        /// Match strings composed of uppercase 26 English letters
        /// </summary>
        public const string UppercaseLetters = @"^[A-Z]+$";
        /// <summary>
        /// Match strings composed of lowercase 26 English letters
        /// </summary>
        public const string LowercaseLetters = @"^[a-z]+$";
        /// <summary>
        /// Match strings composed of numbers and 26 English letters
        /// </summary>
        public const string LettersAndNumber = @"^[A-Za-z0-9]+$";
        /// <summary>
        /// Match any word character including underscore and hyphen
        /// </summary>
        public const string LetterNumberOrLine = @"^[A-Za-z0-9_-]+$";
        /// <summary>
        /// Match any word character including underscore
        /// </summary>
        public const string LettersAndNumberAndLine = @"^\w+$";
    }
}