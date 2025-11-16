using System;
using System.Collections.Generic;
using System.Web.Http;

namespace PasswordGenerator.Controllers
{
    public class PasswordController : ApiController
    {
        private readonly Random _random = new Random();
        private const int DefaultSections = 3;
        private const int SectionLength = 6;

        [HttpGet]
        public IHttpActionResult Get(int? sections = null)
        {
            var sectionCount = sections.GetValueOrDefault(DefaultSections);

            if (sectionCount < DefaultSections)
            {
                sectionCount = DefaultSections;
            }

            var password = GeneratePassword(sectionCount);

            return Ok(new
            {
                password = password
            });
        }

        private string GeneratePassword(int sectionCount)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";

            var totalLetters = sectionCount * SectionLength;
            var chars = new List<char>();

            var hasUpper = false;
            var hasDigit = false;

            for (var letterIndex = 0; letterIndex < totalLetters; letterIndex++)
            {
                // section break
                if (letterIndex > 0 && letterIndex % SectionLength == 0)
                {
                    chars.Add('-');
                }

                double dropChance = (double)(letterIndex + 1) / (totalLetters - 1);

                var needSpecial = !hasUpper || !hasDigit;
                var roll = _random.NextDouble();

                // gacha time!!!
                if (needSpecial && roll < dropChance)
                {
                    // 50/50 time
                    if (!hasUpper && !hasDigit)
                    {
                        if (_random.Next(2) == 0)
                        {
                            chars.Add(uppercase[_random.Next(uppercase.Length)]);
                            hasUpper = true;
                        }
                        else
                        {
                            chars.Add(digits[_random.Next(digits.Length)]);
                            hasDigit = true;
                        }
                    }
                    else if (!hasUpper)
                    {
                        chars.Add(uppercase[_random.Next(uppercase.Length)]);
                        hasUpper = true;
                    }
                    else
                    {
                        chars.Add(digits[_random.Next(digits.Length)]);
                        hasDigit = true;
                    }
                }
                else
                {
                    // lower if not win or both prizes claimed
                    chars.Add(lowercase[_random.Next(lowercase.Length)]);
                }
            }

            return new string(chars.ToArray());
        }
    }
}
