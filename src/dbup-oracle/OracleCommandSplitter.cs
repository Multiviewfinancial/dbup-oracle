using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DbUp.Support;

namespace DbUp.Oracle
{
    public class OracleCommandSplitter
    {
        private char _delimiter;

        [Obsolete]
        public OracleCommandSplitter()
        {
            _delimiter = '/';
        }

        public OracleCommandSplitter(char delimiter)
        {
            _delimiter = delimiter;
        }

        /// <summary>
        /// Splits a script with multiple delimited commands into commands
        /// </summary>
        /// <param name="scriptContents"></param>
        /// <returns></returns>
        public IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            // Split on lines that contain only the delimiter (optionally surrounded by whitespace)
            // Using the cached _compiledRegex for splitting
            var segments = _compiledRegex.Split(scriptContents)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s));
            return segments;
        }
    }
}
