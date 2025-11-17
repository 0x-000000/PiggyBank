using Bank.Models;
using System.Web.Http;

namespace Bank.Controllers
{
    [RoutePrefix("api/bank")]
    public class BankController : ApiController
    {
        //auth "middleware" lmao
        private static bool IsAuthError(string error)
        {
            return string.Equals(error, "Invalid username or password.");
        }

        private BankSystem bank = new BankSystem();

        [HttpPost]
        [Route("signup")]
        public IHttpActionResult SignUp([FromBody] SignUpRequest request)
        {
            var account = bank.SignUp(request?.Username, request?.Password, out var error);

            //failed to sign up case
            if (account == null)
            {
                return BadRequest(error ?? "Unable to create account.");
            }

            return Ok(new
            {
                message = "Account created.",
                username = account.Username,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("deposit")]
        public IHttpActionResult Deposit([FromBody] TransactionRequest request)
        {
            var account = bank.Deposit(request, out var error);

            if (account == null)
            {
                //return a 401 if incorrect cred
                if (IsAuthError(error))
                {
                    return Unauthorized();
                }

                return BadRequest(error ?? "Deposit failed.");
            }

            return Ok(new
            {
                message = "Deposited.",
                account.Username,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("spend")]
        public IHttpActionResult Spend([FromBody] TransactionRequest request)
        {
            var account = bank.Spend(request, out var error);

            if (account == null)
            {
                //return a 401 if incorrect cred
                if (IsAuthError(error))
                {
                    return Unauthorized();
                }

                return BadRequest(error ?? "Spend failed.");
            }

            return Ok(new
            {
                message = "Spent.",
                account.Username,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("balance")]
        public IHttpActionResult Balance([FromBody] BalanceRequest request)
        {
            var account = bank.GetBalance(request, out var error);

            if (account == null)
            {
                //return a 401 if incorrect cred
                if (IsAuthError(error))
                {
                    return Unauthorized();
                }

                return BadRequest(error ?? "Unable to retrieve balance.");
            }

            return Ok(new
            {
                message = "Balance retrieved.",
                account.Username,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("transfer")]
        public IHttpActionResult Transfer([FromBody] TransferRequest request)
        {
            var result = bank.Transfer(request, out var error);

            if (result.from == null || result.to == null)
            {
                //return a 401 if incorrect cred
                if (IsAuthError(error))
                {
                    return Unauthorized();
                }

                return BadRequest(error ?? "Transfer failed.");
            }

            return Ok(new
            {
                message = "Transfer completed.",
                from = new
                {
                    username = result.from.Username,
                    balance = result.from.Balance
                },
                to = new
                {
                    username = result.to.Username,
                    balance = result.to.Balance
                }
            });
        }
    }
}
