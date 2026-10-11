using Microsoft.AspNetCore.Mvc;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Controllers
{
    public class VerificationsKYCController : Controller
    {
        private readonly IVerificationKYCService _verificationKYCService;
        private readonly IClientService _clientService;

        public VerificationsKYCController(IVerificationKYCService verificationKYCService, IClientService clientService)
        {
            _verificationKYCService = verificationKYCService;
            _clientService = clientService;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}