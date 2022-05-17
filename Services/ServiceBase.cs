using AutoMapper;
using Blazored.SessionStorage;
using InCorpApp.Constantes;
using InCorpApp.Security;
using InCorpApp.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Services
{
    public class ServiceBase
    {
        protected ISessionStorageService _sessionStorageService;
        protected SessionService _sessionService;
        protected IMapper _mapper;
        private SessionInfo _si = null;
        private string _accessToken = string.Empty;
        public ServiceBase(
            ISessionStorageService sessionStorageService,
          SessionService sessionService,
          IMapper mapper)

        {

            _sessionStorageService = sessionStorageService;
            _sessionService = sessionService;
            _mapper = mapper;
        }
        public IMapper Mapper { get => _mapper; }
        public async Task<SessionInfo> GetSessionAsync()
        {

            if (_si != null)
            {
                return _si;
            }
            if (_accessToken == null || _accessToken == string.Empty)
            {
                _accessToken = await _sessionStorageService.GetItemAsync<string>(Constante.Accesskey);
            }
            if (_accessToken != null && _accessToken != string.Empty)
            {
                _si = _sessionService.GetById(_accessToken);
                if (_si == null)
                {
                    CustomAuthenticationStateProvider ca = new (_sessionStorageService, null, _sessionService);
                    await ca.MarkUserAsLoggedOut();
                    throw new SessionInfoInvalidaException("Sessão inválida!");
                }
            }
            else
            {
                throw new SessionInfoInvalidaException("Sessão inválida!");

            }
            return _si;
        }
        public async Task<SessionInfo> SetTenantIdToCurrentSession(string tid)
        {
            SessionInfo si = await GetSessionAsync();
            return _sessionService.SetTenantId(si.Id, tid);
        }
        public void RemoveGuestSession(string id)
        {
            _sessionService.RemoveSession(id);
        }
    }
}
