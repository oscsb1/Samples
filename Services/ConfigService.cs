using System;
using Microsoft.Extensions.Configuration;

namespace InCorpApp.Services
{
    public class ConfigService
    {
        private readonly IConfiguration _configuration;
        private string _securityURL = string.Empty;
        private string _baseyURL = string.Empty;
        private string _mailServer = string.Empty;
        private string _fileDBPath = string.Empty;
        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetFileDBPath()
        {
            if (_fileDBPath == string.Empty)
            {
                _fileDBPath = _configuration["SmartFOX:FileDBPath"];
            }
            return _fileDBPath;
        }
        public string GetSecurityUrl()
        {
            if (_securityURL == string.Empty)
            {
                _securityURL = _configuration["SmartFOX:SecurityURL"];
            }
            return _securityURL;
        }
        public string GetBaseUrl()
        {
            if (_baseyURL == string.Empty)
            {
                _baseyURL = _configuration["SmartFOX:BaseURL"];
            }
            return _baseyURL;
        }
        public string GetMailServer()
        {
            if (_mailServer == string.Empty)
            {
                _mailServer = _configuration["SmartFOX:MailServer"];
                if (_mailServer == null || _mailServer == string.Empty)
                {
                    _mailServer = "xxxmail.sf21app.com";
                }
            }
            return _mailServer;
        }
    }
}
