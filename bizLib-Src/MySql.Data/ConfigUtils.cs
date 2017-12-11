// Copyright © 2016, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA


namespace MySql.Data.MySqlClient
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    public class ConfigUtils
    {
      private IConfiguration _configuration; 
      public ConfigUtils(string settingsFile)
      {
          var builder = new ConfigurationBuilder()                        
                        .AddJsonFile(settingsFile, false);        
          _configuration = builder.Build();
      }

      public string GetValue(string section)
      {
          var value = _configuration?.GetSection(section).Value;
          return value;
      }

      public string GetPort()
      {
          return GetValue("MySql:Data:Port");
      }
    }

    public enum PlatformRunning
    {
        Windows,
        Linux,
        OsX,
        Unsupported
    }

    public static class PlatformUtils
    {
      public static PlatformRunning OsPlatform
        { 
           get {
                if (File.Exists(@"/proc/sys/kernel/ostype"))
                {
                    string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                    if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                        return PlatformRunning.Linux;
                }
                else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
                    return PlatformRunning.OsX;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return PlatformRunning.Windows;
                return PlatformRunning.Unsupported;
            }
        }

        public static string OSDescription()
        {
            return RuntimeInformation.OSDescription;
        }
    }

}
