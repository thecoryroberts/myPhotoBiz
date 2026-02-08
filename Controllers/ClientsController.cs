
// Controllers/ClientsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using MyPhotoBiz.Data;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;
using MyPhotoBiz.Services;
using MyPhotoBiz.Helpers;

namespace MyPhotoBiz.Controllers
{
    /// <summary>
    /// Handles HTTP requests for clients.
    /// </summary>
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IActivityService _activityService;

        public ClientsController(IClientService clientService, UserManager<ApplicationUser> userManager,
            ApplicationDbContext context, IActivityService activityService)
        {
            _clientService = clientService;
            _userManager = userManager;
            _context = context;
            _activityService = activityService;
        }

        private ClientDetailsViewModel MapToClientDetailsViewModel(ClientProfile clientProfile)
        {
            return new ClientDetailsViewModel
            {
                Id = clientProfile.Id,
                FirstName = clientProfile.User?.FirstName ?? "",
                LastName = clientProfile.User?.LastName ?? "",
                Email = clientProfile.User?.Email ?? "",
                PhoneNumber = clientProfile.PhoneNumber,
                Address = clientProfile.Address,
                Notes = clientProfile.Notes,
                UpdatedDate = clientProfile.UpdatedDate,
                CreatedDate = clientProfile.CreatedDate,
                User = clientProfile.User,
                PhotoShootCount = clientProfile.PhotoShoots?.Count ?? 0,
                InvoiceCount = clientProfile.Invoices?.Count ?? 0,
                TotalRevenue = clientProfile.Invoices?.Sum(i => i.Amount + i.Tax) ?? 0m,
                PhotoShoots = clientProfile.PhotoShoots?.Select(ps => new PhotoShootViewModel
                {
                    Id = ps.Id,
                    Title = ps.Title,
                    ClientId = ps.ClientProfileId,
                    ScheduledDate = ps.ScheduledDate,
                    UpdatedDate = ps.UpdatedDate,
                    Location = ps.Location,
                    Status = ps.Status,
                    Price = ps.Price,
                    Notes = ps.Notes,
                    DurationHours = ps.DurationHours,
                    DurationMinutes = ps.DurationMinutes
                }).ToList() ?? new List<PhotoShootViewModel>(),
                Invoices = clientProfile.Invoices?.ToList() ?? new List<Invoice>(),
                ClientBadges = clientProfile.ClientBadges?.ToList() ?? new List<ClientBadge>(),
                Contracts = clientProfile.Contracts?.ToList() ?? new List<Contract>()
            };
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllClientsAsync();
            return View(clients);
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Details(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }

            var model = MapToClientDetailsViewModel(clientProfile);
            return View("Details", model);
        }

        [Authorize(Roles = "Admin,Photographer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(model);
                }

                var temporaryPassword = PasswordGenerator.GenerateSecurePassword();

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, temporaryPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");

                    // Create ClientProfile
                    var clientProfile = new ClientProfile
                    {
                        UserId = user.Id,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        Notes = model.Notes ?? string.Empty,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };

                    await _clientService.CreateClientAsync(clientProfile);

                    // Auto-award "New User" badge
                    await AwardNewUserBadgeAsync(clientProfile.Id);

                    // Log activity
                    await _activityService.LogActivityAsync("Created", "Client", clientProfile.Id,
                        $"{model.FirstName} {model.LastName}", null, _userManager.GetUserId(User));

                    TempData["SuccessMessage"] = $"Client created successfully. Temporary password: {temporaryPassword}";
                    TempData["PasswordWarning"] = "Please share this password securely with the client. It will not be shown again.";

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Edit(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }

            var model = new EditClientViewModel
            {
                Id = clientProfile.Id,
                FirstName = clientProfile.User?.FirstName ?? "",
                LastName = clientProfile.User?.LastName ?? "",
                Email = clientProfile.User?.Email ?? "",
                PhoneNumber = clientProfile.PhoneNumber,
                Address = clientProfile.Address,
                Notes = clientProfile.Notes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> Edit(int id, EditClientViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var clientProfile = await _clientService.GetClientByIdAsync(id);
                if (clientProfile == null)
                {
                    return NotFound();
                }

                // Update ApplicationUser fields
                if (clientProfile.User != null)
                {
                    clientProfile.User.FirstName = model.FirstName;
                    clientProfile.User.LastName = model.LastName;
                    await _userManager.UpdateAsync(clientProfile.User);
                }

                // Update ClientProfile fields
                clientProfile.PhoneNumber = model.PhoneNumber;
                clientProfile.Address = model.Address;
                clientProfile.Notes = model.Notes ?? string.Empty;

                await _clientService.UpdateClientAsync(clientProfile);

                // Log activity
                await _activityService.LogActivityAsync("Updated", "Client", clientProfile.Id,
                    $"{model.FirstName} {model.LastName}", null, _userManager.GetUserId(User));

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }
            return View(clientProfile);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clientProfile = await _clientService.GetClientByIdAsync(id);
            var clientName = clientProfile != null
                ? $"{clientProfile.User?.FirstName} {clientProfile.User?.LastName}"
                : $"ID: {id}";
            await _clientService.SoftDeleteClientAsync(id);
            // Log activity
            await _activityService.LogActivityAsync("Deleted", "Client", id,
                clientName, null, _userManager.GetUserId(User));

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Client")]
        public IActionResult MyProfile()
        {
            // Redirect to the Identity area Razor Page that manages user details.
            // The Identity `UserDetails` page already handles displaying and editing the current user's profile.
            return RedirectToPage("/Account/Manage/UserDetails", new { area = "Identity" });
        }

        /// <summary>
        /// Client portal: View assigned questionnaires and download documents.
        /// </summary>
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyQuestionnaires()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignments = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .Include(a => a.AssignedByUser)
                .Where(a => a.AssignedToUserId == userId)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync();

            return View(assignments);
        }

        /// <summary>
        /// Client portal: Answer a text-based questionnaire.
        /// </summary>
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AnswerQuestionnaire(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignment = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssignedToUserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(assignment.QuestionnaireTemplate?.QuestionText))
            {
                TempData["Error"] = "This questionnaire does not have text-based questions. Please download the document instead.";
                return RedirectToAction(nameof(MyQuestionnaires));
            }

            var questionText = assignment.QuestionnaireTemplate?.QuestionText ?? string.Empty;
            var items = BuildQuestionItems(questionText, assignment.ResponseText, out var legacyResponse);

            var viewModel = new QuestionnaireResponseViewModel
            {
                AssignmentId = assignment.Id,
                QuestionnaireName = assignment.QuestionnaireTemplate?.Name ?? "Questionnaire",
                QuestionText = questionText,
                Items = items,
                LegacyResponseText = legacyResponse,
                IsCompleted = assignment.Status == QuestionnaireAssignmentStatus.Completed
            };

            return View(viewModel);
        }

        /// <summary>
        /// Client portal: Submit questionnaire responses.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AnswerQuestionnaire(QuestionnaireResponseViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignment = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .FirstOrDefaultAsync(a => a.Id == model.AssignmentId && a.AssignedToUserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(assignment.QuestionnaireTemplate?.QuestionText))
            {
                TempData["Error"] = "This questionnaire does not have text-based questions. Please download the document instead.";
                return RedirectToAction(nameof(MyQuestionnaires));
            }

            var submittedItems = model.Items ?? new List<QuestionnaireResponseItemViewModel>();
            model.Items = submittedItems;

            if (submittedItems.Count == 0)
            {
                ModelState.AddModelError("", "No questions were found for this questionnaire.");
            }

            if (!ModelState.IsValid)
            {
                model.QuestionnaireName = assignment.QuestionnaireTemplate?.Name ?? "Questionnaire";
                model.QuestionText = assignment.QuestionnaireTemplate?.QuestionText ?? string.Empty;
                model.IsCompleted = assignment.Status == QuestionnaireAssignmentStatus.Completed;
                if (model.Items.Count == 0)
                {
                    model.Items = BuildQuestionItems(model.QuestionText, assignment.ResponseText, out var legacyResponse);
                    model.LegacyResponseText = legacyResponse;
                }
                return View(model);
            }

            var normalizedItems = new List<QuestionnaireResponseItemViewModel>();
            for (var i = 0; i < submittedItems.Count; i++)
            {
                var item = submittedItems[i];
                if (string.IsNullOrWhiteSpace(item.Question))
                {
                    continue;
                }

                var normalizedType = NormalizeQuestionType(item.Type);
                var normalizedOptions = NormalizeOptions(item.Options);
                if ((normalizedType == "select" || normalizedType == "multiselect") && normalizedOptions.Count == 0)
                {
                    normalizedType = "text";
                }

                var normalizedItem = new QuestionnaireResponseItemViewModel
                {
                    Section = (item.Section ?? string.Empty).Trim(),
                    Question = item.Question.Trim(),
                    Type = normalizedType,
                    Options = normalizedOptions,
                    Answer = (item.Answer ?? string.Empty).Trim(),
                    AnswerBool = item.AnswerBool,
                    AnswerList = NormalizeOptions(item.AnswerList)
                };

                NormalizeAnswer(normalizedItem);

                if (!IsAnswerValid(normalizedItem))
                {
                    var errorKey = GetAnswerValidationKey(normalizedItem.Type, i);
                    ModelState.AddModelError(errorKey, "This field is required.");
                }

                normalizedItems.Add(normalizedItem);
            }

            if (normalizedItems.Count == 0)
            {
                ModelState.AddModelError("", "No questions were found for this questionnaire.");
            }

            if (!ModelState.IsValid)
            {
                model.QuestionnaireName = assignment.QuestionnaireTemplate?.Name ?? "Questionnaire";
                model.QuestionText = assignment.QuestionnaireTemplate?.QuestionText ?? string.Empty;
                model.IsCompleted = assignment.Status == QuestionnaireAssignmentStatus.Completed;
                model.Items = normalizedItems;
                return View(model);
            }

            assignment.ResponseText = JsonSerializer.Serialize(normalizedItems);
            assignment.Status = QuestionnaireAssignmentStatus.Completed;
            assignment.CompletedDate ??= DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Questionnaire submitted successfully!";
            return RedirectToAction(nameof(MyQuestionnaires));
        }

        /// <summary>
        /// Download questionnaire document for an assignment.
        /// </summary>
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> DownloadQuestionnaire(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignment = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssignedToUserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            var template = assignment.QuestionnaireTemplate;
            if (template == null || string.IsNullOrEmpty(template.DocumentPath))
            {
                TempData["Error"] = "No document available for this questionnaire.";
                return RedirectToAction(nameof(MyQuestionnaires));
            }

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.GetFullPath(Path.Combine(webRootPath, template.DocumentPath.TrimStart('/', '\\')));
            
            // Prevent path traversal attacks
            if (!fullPath.StartsWith(webRootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = "Document file not found.";
                return RedirectToAction(nameof(MyQuestionnaires));
            }

            var contentType = template.DocumentType switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "doc" => "application/msword",
                _ => "application/octet-stream"
            };

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, contentType, template.OriginalFileName);
        }

        /// <summary>
        /// Mark a questionnaire assignment as completed.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MarkQuestionnaireComplete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var assignment = await _context.QuestionnaireAssignments
                .Include(a => a.QuestionnaireTemplate)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssignedToUserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(assignment.QuestionnaireTemplate?.QuestionText) &&
                !HasAllAnswers(assignment.ResponseText))
            {
                TempData["Error"] = "Please answer the questionnaire before marking it complete.";
                return RedirectToAction(nameof(MyQuestionnaires));
            }

            assignment.Status = QuestionnaireAssignmentStatus.Completed;
            assignment.CompletedDate ??= DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Questionnaire marked as complete!";
            return RedirectToAction(nameof(MyQuestionnaires));
        }

        private static List<QuestionnaireResponseItemViewModel> BuildQuestionItems(
            string questionText,
            string? responseText,
            out string? legacyResponseText)
        {
            legacyResponseText = null;
            var items = ParseQuestionText(questionText);

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return items;
            }

            var existingResponses = TryDeserializeResponses(responseText);
            if (existingResponses == null)
            {
                legacyResponseText = responseText;
                return items;
            }

            var responseMap = new Dictionary<string, QuestionnaireResponseItemViewModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var response in existingResponses)
            {
                if (string.IsNullOrWhiteSpace(response.Question))
                {
                    continue;
                }

                var key = BuildQuestionKey(response.Section, response.Question);
                if (!responseMap.ContainsKey(key))
                {
                    responseMap[key] = response;
                }
            }

            foreach (var item in items)
            {
                var key = BuildQuestionKey(item.Section, item.Question);
                if (responseMap.TryGetValue(key, out var response))
                {
                    ApplyResponse(item, response);
                }
            }

            return items;
        }

        private static List<QuestionnaireResponseItemViewModel> ParseQuestionText(string questionText)
        {
            var items = new List<QuestionnaireResponseItemViewModel>();
            if (string.IsNullOrWhiteSpace(questionText))
            {
                return items;
            }

            var lines = questionText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var currentSection = "Questions";
            var ignoreSection = false;
            var seenQuestion = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var isBullet = line.StartsWith("-", StringComparison.Ordinal) || line.StartsWith("•", StringComparison.Ordinal);
                if (!isBullet)
                {
                    var header = line.TrimEnd(':').Trim();
                    if (!seenQuestion &&
                        (header.Contains("Questionnaire", StringComparison.OrdinalIgnoreCase) ||
                         header.Contains("Acknowledgement", StringComparison.OrdinalIgnoreCase)))
                    {
                        currentSection = "Questions";
                        ignoreSection = false;
                        continue;
                    }

                    currentSection = string.IsNullOrWhiteSpace(header) ? "Questions" : header;
                    ignoreSection = IsNonQuestionSection(currentSection);
                    continue;
                }

                if (ignoreSection)
                {
                    continue;
                }

                var question = line.TrimStart('-', '•', ' ').Trim();
                if (string.IsNullOrWhiteSpace(question))
                {
                    continue;
                }

                if (question.EndsWith(":", StringComparison.Ordinal))
                {
                    question = question[..^1].Trim();
                }

                var (type, options, cleanedQuestion) = ExtractTypeAndOptions(question);
                var resolvedType = NormalizeQuestionType(type);
                if (string.IsNullOrWhiteSpace(cleanedQuestion))
                {
                    continue;
                }

                if (resolvedType == "text")
                {
                    resolvedType = GuessQuestionType(cleanedQuestion, currentSection);
                }

                if ((resolvedType == "select" || resolvedType == "multiselect") && options.Count == 0)
                {
                    resolvedType = "text";
                }

                items.Add(new QuestionnaireResponseItemViewModel
                {
                    Section = currentSection,
                    Question = cleanedQuestion,
                    Type = resolvedType,
                    Options = options
                });
                seenQuestion = true;
            }

            return items;
        }

        private static bool IsNonQuestionSection(string header)
        {
            return header.Equals("Purpose", StringComparison.OrdinalIgnoreCase) ||
                   header.Equals("When used", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildQuestionKey(string? section, string? question)
        {
            return $"{section ?? string.Empty}||{question ?? string.Empty}";
        }

        private static void ApplyResponse(
            QuestionnaireResponseItemViewModel item,
            QuestionnaireResponseItemViewModel response)
        {
            if (!string.IsNullOrWhiteSpace(response.Type))
            {
                item.Type = NormalizeQuestionType(response.Type);
            }

            if (item.Options.Count == 0 && response.Options?.Count > 0)
            {
                item.Options = NormalizeOptions(response.Options);
            }

            switch (NormalizeQuestionType(item.Type))
            {
                case "yesno":
                case "checkbox":
                    if (response.AnswerBool.HasValue)
                    {
                        item.AnswerBool = response.AnswerBool;
                    }
                    else if (!string.IsNullOrWhiteSpace(response.Answer))
                    {
                        item.AnswerBool = response.Answer.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase);
                    }
                    break;
                case "multiselect":
                    if (response.AnswerList != null && response.AnswerList.Count > 0)
                    {
                        item.AnswerList = NormalizeOptions(response.AnswerList);
                    }
                    else if (!string.IsNullOrWhiteSpace(response.Answer))
                    {
                        item.AnswerList = NormalizeOptions(response.Answer.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    }
                    break;
                default:
                    item.Answer = response.Answer ?? string.Empty;
                    break;
            }
        }

        private static List<QuestionnaireResponseItemViewModel>? TryDeserializeResponses(string responseText)
        {
            try
            {
                return JsonSerializer.Deserialize<List<QuestionnaireResponseItemViewModel>>(responseText);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static bool HasAllAnswers(string? responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return false;
            }

            var items = TryDeserializeResponses(responseText);
            if (items == null || items.Count == 0)
            {
                return true;
            }

            return items.All(IsAnswerValid);
        }

        private static (string? type, List<string> options, string cleanedQuestion) ExtractTypeAndOptions(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return (null, new List<string>(), string.Empty);
            }

            var match = Regex.Match(question, @"\[(?<type>[A-Za-z]+)(:(?<opts>[^\]]+))?\]\s*$");
            if (!match.Success)
            {
                return (null, new List<string>(), question.Trim());
            }

            var rawType = match.Groups["type"].Value;
            var rawOptions = match.Groups["opts"].Value;
            var cleaned = question[..match.Index].Trim();
            return (rawType, ParseOptions(rawOptions), cleaned);
        }

        private static List<string> ParseOptions(string? rawOptions)
        {
            if (string.IsNullOrWhiteSpace(rawOptions))
            {
                return new List<string>();
            }

            var separators = rawOptions.Contains('|', StringComparison.Ordinal)
                ? new[] { '|' }
                : new[] { ',', ';' };

            return rawOptions
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(option => option.Trim())
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> NormalizeOptions(IEnumerable<string>? options)
        {
            if (options == null)
            {
                return new List<string>();
            }

            return options
                .Select(option => option?.Trim() ?? string.Empty)
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizeQuestionType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return "text";
            }

            var normalized = type.Trim().ToLowerInvariant();
            return normalized switch
            {
                "yesno" => "yesno",
                "yes/no" => "yesno",
                "bool" => "yesno",
                "boolean" => "yesno",
                "checkbox" => "checkbox",
                "check" => "checkbox",
                "multiselect" => "multiselect",
                "multi" => "multiselect",
                "checkboxes" => "multiselect",
                "select" => "select",
                "dropdown" => "select",
                "textarea" => "textarea",
                "longtext" => "textarea",
                "paragraph" => "textarea",
                "email" => "email",
                "phone" => "tel",
                "tel" => "tel",
                "telephone" => "tel",
                "date" => "date",
                "datetime" => "date",
                "number" => "number",
                "numeric" => "number",
                _ => "text"
            };
        }

        private static string GuessQuestionType(string question, string? section)
        {
            var lower = question.ToLowerInvariant();
            var sectionLower = section?.ToLowerInvariant() ?? string.Empty;
            if (sectionLower.Contains("key points") ||
                sectionLower.Contains("usage scope") ||
                sectionLower.Contains("consent"))
            {
                return "checkbox";
            }

            if (lower.Contains("date"))
            {
                return "date";
            }

            if (lower.Contains("email"))
            {
                return "email";
            }

            if (lower.Contains("phone"))
            {
                return "tel";
            }

            if (lower.Contains("how many") || lower.Contains("number of") || lower.Contains("quantity"))
            {
                return "number";
            }

            if (lower.Contains("permission") || lower.Contains("consent"))
            {
                return "yesno";
            }

            if (IsYesNoQuestion(lower))
            {
                return "yesno";
            }

            if (lower.Contains("describe") || lower.Contains("explain") || lower.Contains("tell us") ||
                lower.Contains("what ") || lower.Contains("why ") || lower.Contains("how "))
            {
                return "textarea";
            }

            return "text";
        }

        private static bool IsYesNoQuestion(string lowerQuestion)
        {
            if (!lowerQuestion.EndsWith("?"))
            {
                return false;
            }

            var startsWith = lowerQuestion.StartsWith("do ") ||
                             lowerQuestion.StartsWith("does ") ||
                             lowerQuestion.StartsWith("did ") ||
                             lowerQuestion.StartsWith("is ") ||
                             lowerQuestion.StartsWith("are ") ||
                             lowerQuestion.StartsWith("was ") ||
                             lowerQuestion.StartsWith("were ") ||
                             lowerQuestion.StartsWith("can ") ||
                             lowerQuestion.StartsWith("could ") ||
                             lowerQuestion.StartsWith("will ") ||
                             lowerQuestion.StartsWith("would ") ||
                             lowerQuestion.StartsWith("should ") ||
                             lowerQuestion.StartsWith("has ") ||
                             lowerQuestion.StartsWith("have ");

            if (!startsWith)
            {
                return false;
            }

            return !lowerQuestion.StartsWith("what ") &&
                   !lowerQuestion.StartsWith("why ") &&
                   !lowerQuestion.StartsWith("how ");
        }

        private static bool IsAnswerValid(QuestionnaireResponseItemViewModel item)
        {
            var type = NormalizeQuestionType(item.Type);
            switch (type)
            {
                case "yesno":
                case "checkbox":
                    return item.AnswerBool.HasValue || !string.IsNullOrWhiteSpace(item.Answer);
                case "multiselect":
                    return (item.AnswerList != null && item.AnswerList.Any()) || !string.IsNullOrWhiteSpace(item.Answer);
                case "select":
                case "date":
                case "email":
                case "tel":
                case "number":
                case "textarea":
                case "text":
                default:
                    return !string.IsNullOrWhiteSpace(item.Answer);
            }
        }

        private static void NormalizeAnswer(QuestionnaireResponseItemViewModel item)
        {
            var type = NormalizeQuestionType(item.Type);
            switch (type)
            {
                case "yesno":
                    if (item.AnswerBool.HasValue && string.IsNullOrWhiteSpace(item.Answer))
                    {
                        item.Answer = item.AnswerBool.Value ? "Yes" : "No";
                    }
                    break;
                case "checkbox":
                    if (item.AnswerBool.HasValue && string.IsNullOrWhiteSpace(item.Answer))
                    {
                        item.Answer = item.AnswerBool.Value ? "Checked" : "Unchecked";
                    }
                    break;
                case "multiselect":
                    if (item.AnswerList != null && item.AnswerList.Any())
                    {
                        item.Answer = string.Join(", ", item.AnswerList);
                    }
                    break;
            }
        }

        private static string GetAnswerValidationKey(string type, int index)
        {
            var normalized = NormalizeQuestionType(type);
            return normalized switch
            {
                "yesno" => $"Items[{index}].AnswerBool",
                "checkbox" => $"Items[{index}].AnswerBool",
                "multiselect" => $"Items[{index}].AnswerList",
                _ => $"Items[{index}].Answer"
            };
        }

        // API endpoint for getting clients list (used by manage access modal)
        [HttpGet]
        [Route("api/clients")]
        [Authorize(Roles = "Admin,Photographer")]
        public async Task<IActionResult> GetClientsApi()
        {
            var clients = await _clientService.GetAllClientsAsync();
            var result = clients.Select(c => new
            {
                id = c.Id,
                firstName = c.User?.FirstName ?? "",
                lastName = c.User?.LastName ?? "",
                email = c.User?.Email ?? ""
            });
            return Json(result);
        }

        private async Task AwardNewUserBadgeAsync(int clientProfileId)
        {
            try
            {
                // Find the "New User" badge
                var newUserBadge = await _context.Badges
                    .FirstOrDefaultAsync(b => b.Name == "New User" && b.IsActive);

                if (newUserBadge == null)
                    return; // Badge doesn't exist or isn't active, skip silently

                // Check if client already has this badge
                var hasBadge = await _context.ClientBadges
                    .AnyAsync(cb => cb.ClientProfileId == clientProfileId && cb.BadgeId == newUserBadge.Id);

                if (hasBadge)
                    return; // Already has the badge

                // Award the badge
                var clientBadge = new ClientBadge
                {
                    ClientProfileId = clientProfileId,
                    BadgeId = newUserBadge.Id,
                    EarnedDate = DateTime.UtcNow,
                    Notes = "Auto-awarded on account creation"
                };

                _context.ClientBadges.Add(clientBadge);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log the error but don't fail the client creation
                // Badge awarding is a non-critical feature
            }
        }
    }
}
