using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;

namespace MyPhotoBiz.Controllers
{
   // [Authorize]

    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
    public ContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts.ToListAsync();
            return View(contracts);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contract);
        }

        public async Task<IActionResult> Sign(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> Sign(int id, string signatureBase64)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            var signaturePath = SaveSignature(signatureBase64);
            contract.SignatureImagePath = signaturePath;
            contract.SignedDate = DateTime.UtcNow;
            contract.Status = ContractStatus.Signed;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private string SaveSignature(string base64)
        {
            var bytes = Convert.FromBase64String(base64.Split(',')[1]);
            var fileName = $"{Guid.NewGuid()}.png";
            var path = Path.Combine("wwwroot/signatures", fileName);
            System.IO.File.WriteAllBytes(path, bytes);
            return $"/signatures/{fileName}";
        }
    }
}