using ThriftMedia.Contracts.Dto;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Auth.ResolveSignIn;

public record ResolveSignInCommand(ResolveSignInRequest Request) : IRequest<SignInResultDto>;
