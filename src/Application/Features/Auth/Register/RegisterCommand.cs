using MediatR;
using Application.Features.Auth.Register;

namespace Application.Features.Auth.Register;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<RegisterResponse>;
