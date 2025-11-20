using Bank.Models;
using System.Web;
using System.Web.Http;

namespace Bank.Controllers
{
    [RoutePrefix("api/bank")]
    public class BankController : ApiController
    {
        //auth "middleware" lmao
        private readonly BankSystem bank = new BankSystem();

        private IHttpActionResult RequireActor(out BankAccount actor)
        {
            var request = HttpContext.Current?.Request;
            var username = request?.Cookies["user"]?.Value;

            if (string.IsNullOrWhiteSpace(username))
            {
                actor = null;
                return Unauthorized();
            }

            actor = bank.GetAccount(username);
            if (actor == null)
            {
                return Unauthorized();
            }

            return null;
        }

        [HttpPost]
        [Route("signup")]
        public IHttpActionResult SignUp([FromBody] SignUpRequest request)
        {
            var account = bank.SignUp(request?.Username, request?.Password, request?.AccountType, out var error);

            //failed to sign up case
            if (account == null)
            {
                return BadRequest(error ?? "Unable to create account.");
            }

            return Ok(new
            {
                message = "Account created.",
                username = account.Username,
                accountType = account.AccountType,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("deposit")]
        public IHttpActionResult Deposit([FromBody] TransactionRequest request)
        {
            var fail = RequireActor(out var actor);
            if (fail != null)
            {
                return fail;
            }

            var account = bank.Deposit(actor, request, out var error);

            if (account == null)
            {
                return BadRequest(error ?? "Deposit failed.");
            }

            return Ok(new
            {
                message = "Deposited.",
                username = account.Username,
                accountType = account.AccountType,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("spend")]
        public IHttpActionResult Spend([FromBody] TransactionRequest request)
        {
            var fail = RequireActor(out var actor);
            if (fail != null)
            {
                return fail;
            }

            var account = bank.Spend(actor, request, out var error);

            if (account == null)
            {
                return BadRequest(error ?? "Spend failed.");
            }

            return Ok(new
            {
                message = "Spent.",
                username = account.Username,
                accountType = account.AccountType,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("balance")]
        public IHttpActionResult Balance([FromBody] BalanceRequest request)
        {
            var fail = RequireActor(out var actor);
            if (fail != null)
            {
                return fail;
            }

            var account = bank.GetBalance(actor, request, out var error);

            if (account == null)
            {
                return BadRequest(error ?? "Unable to retrieve balance.");
            }

            return Ok(new
            {
                message = "Balance retrieved.",
                username = account.Username,
                accountType = account.AccountType,
                balance = account.Balance
            });
        }

        [HttpPost]
        [Route("transfer")]
        public IHttpActionResult Transfer([FromBody] TransferRequest request)
        {
            var fail = RequireActor(out var actor);
            if (fail != null)
            {
                return fail;
            }

            var result = bank.Transfer(actor, request, out var error);

            if (result.from == null || result.to == null)
            {
                return BadRequest(error ?? "Transfer failed.");
            }

            return Ok(new
            {
                message = "Transfer completed.",
                from = new
                {
                    username = result.from.Username,
                    accountType = result.from.AccountType,
                    balance = result.from.Balance
                },
                to = new
                {
                    username = result.to.Username,
                    accountType = result.to.AccountType,
                    balance = result.to.Balance
                }
            });
        }

        [HttpPost]
        [Route("promote")]
        public IHttpActionResult Promote([FromBody] BalanceRequest request)
        {
            var fail = RequireActor(out var actor);
            if (fail != null)
            {
                return fail;
            }

            var result = bank.Promote(actor, request?.TargetUsername, out var error);
            if (result == null)
            {
                return BadRequest(error ?? "Promote failed.");
            }

            return Ok(new
            {
                message = "Promoted.",
                username = result.Username,
                accountType = result.AccountType
            });
        }

        [HttpPost]
        [Route("demote")]
        public IHttpActionResult Demote([FromBody] BalanceRequest request)
        {
            var fail = RequireActor(out var actor);
            if (fail != null)
            {
                return fail;
            }

            var result = bank.Demote(actor, request?.TargetUsername, out var error);
            if (result == null)
            {
                return BadRequest(error ?? "Demote failed.");
            }

            return Ok(new
            {
                message = "Demoted.",
                username = result.Username,
                accountType = result.AccountType
            });
        }
    }
}
