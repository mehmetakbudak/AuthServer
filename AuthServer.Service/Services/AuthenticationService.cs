using AuthServer.Core;
using AuthServer.Core.Configuration;
using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenRepository;

        public AuthenticationService(
            IOptions<List<Client>> optionsClient,
            ITokenService tokenService,
            UserManager<UserApp> userManager,
            IUnitOfWork unitOfWork,
            IGenericRepository<UserRefreshToken> userRefreshTokenRepository)
        {
            _clients = optionsClient.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenRepository = userRefreshTokenRepository;
        }

        public async Task<Response<TokenDto>> CreateToken(LoginDto model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Response<TokenDto>.Fail("Email or password is wrong", 400, true);
            }

            var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!checkPassword)
            {
                return Response<TokenDto>.Fail("Email or password is wrong", 400, true);
            }
            var token = await _tokenService.CreateToken(user);
            var userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();

            if (userRefreshToken == null)
            {
                await _userRefreshTokenRepository.AddAsync(new UserRefreshToken
                {
                    Code = token.RefreshToken,
                    Expiration = token.RefreshTokenExpiration,
                    UserId = user.Id
                });
            }
            else
            {
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
                userRefreshToken.Code = token.RefreshToken;
            }
            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto model)
        {
            var client = _clients.FirstOrDefault(x => x.Id == model.ClientId && x.Secret == model.ClientSecret);

            if (client == null)
            {
                return Response<ClientTokenDto>.Fail("", 404, true);
            }

            var token = _tokenService.CreateTokenByClient(client);

            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var entityRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code == refreshToken).FirstOrDefaultAsync();

            if (entityRefreshToken == null)
            {
                return Response<TokenDto>.Fail("Refresh token not found", 404, true);
            }

            var user = await _userManager.FindByIdAsync(entityRefreshToken.UserId);

            if (user == null)
            {
                return Response<TokenDto>.Fail("User not found", 404, true);
            }

            var token = await _tokenService.CreateToken(user);
            entityRefreshToken.Code = token.RefreshToken;
            entityRefreshToken.Expiration = token.RefreshTokenExpiration;

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(token, 200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var entityRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code != refreshToken).FirstOrDefaultAsync();

            if (entityRefreshToken == null)
            {
                return Response<NoDataDto>.Fail("Refresh token not found.", 404, true);
            }

            _userRefreshTokenRepository.Remove(entityRefreshToken);

            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
