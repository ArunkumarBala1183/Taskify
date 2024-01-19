using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Task_Management.Models.Templete;

namespace Task_Management.Models.JWT
{
    public class JWTOperations
    {
        public bool authenticated = false;
        private readonly IConfiguration _config;
        public JWTOperations(IConfiguration config)
        {
            _config = config;
        }

        public string generateToken(object userDetails)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var audience = "\"" + _config["Jwt:Audience"] + "\"";
            var issuer = "\"" + _config["Jwt:Issuer"] + "\"";

            var claims = new List<Claim>();

            if (userDetails.GetType() == typeof(Manager))
            {
                Manager manager = (Manager)userDetails;

                claims.Add(new Claim(ClaimTypes.NameIdentifier, manager.managerId));
                claims.Add(new Claim(ClaimTypes.Role, manager.role));
                
            }
            else
            {
                Employee employee = (Employee) userDetails;
                claims.Add(new Claim(ClaimTypes.NameIdentifier, employee.employeeId));
                claims.Add(new Claim(ClaimTypes.Role, employee.role));
            }


            var token = new JwtSecurityToken(
                audience,
                issuer,
                claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
                );

            this.authenticated = true;
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public ClaimsPrincipal validateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);


            var validateParameters = new TokenValidationParameters
            {
                ValidAudience = JsonSerializer.Serialize(_config["Jwt:Audience"]),
                ValidIssuer = JsonSerializer.Serialize(_config["Jwt:Issuer"]),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validateParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public object validateRole(string token)
        {
            var claims  = this.validateToken(token);
            if (claims != null && claims.Claims.Any(c=> c.Type == ClaimTypes.Role && c.Value == "Manager"))
            {
                return true;
            }
            else if(claims != null)
            {
                return false;
            }

            return _config.GetSection("JWT")["fails"];

        }
    }
}
