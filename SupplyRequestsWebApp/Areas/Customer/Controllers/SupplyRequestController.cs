using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupplyManagement.Helpers;
using SupplyManagement.Models;
using SupplyManagement.Models.Enums;
using SupplyManagement.Models.Interfaces;
using SupplyManagement.Models.ViewModels;
using SupplyManagement.Services;
using System.Data.SqlTypes;

namespace SupplyManagement.WebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class SupplyRequestController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<SupplyRequestController> _logger;
        private readonly UserHelper _userHelper;
        private readonly IHubContext<NotificationHub> _hubContext;


        public SupplyRequestController(IApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<SupplyRequestController> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _userHelper = new UserHelper(userManager, logger);
            _hubContext = hubContext;
        }

        // GET: Supply Requests
        [HttpGet]
        [Authorize(Roles = "User,Manager,Courier")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Check if the supply requests context is null
                if (_context.SupplyRequests == null)
                {
                    // Log an error if the supply requests context is null
                    _logger.LogError("Supply requests context is null.");
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.SupplyRequests' is null");
                }

                // Initialize a list to store supply requests
                List<SupplyRequest> requests = [];

                // Get the current user id
                var (userId, _) = await this._userHelper.GetCurrentUserAsync(User).ConfigureAwait(false);

                if (User.IsInRole("Manager"))
                {
                    // Retrieve supply requests approved by the current user
                    _logger.LogInformation("Get requests approved by Manager with id: {id}", userId);
                    requests = await _context.SupplyRequests
                        .Include(r => r.CreatedBy)
                        .Include(r => r.ApprovedBy)
                        .Include(r => r.DeliveredBy)
                        .Where(r => r.ApprovedBy != null && r.ApprovedBy.Id == userId)
                        .ToListAsync();
                }
                else if (User.IsInRole("Courier"))
                {
                    _logger.LogInformation("Get requests to deliver for Courier with id: {id}", userId);
                    requests = await _context.SupplyRequests
                        .Include(r => r.CreatedBy)
                        .Include(r => r.ApprovedBy)
                        .Include(r => r.DeliveredBy)
                        .Where(r => (r.Status == SupplyRequestStatuses.PendingDelivery
                        || r.Status == SupplyRequestStatuses.Delivered
                        || r.Status == SupplyRequestStatuses.DeliveredWithClaims
                        || r.Status == SupplyRequestStatuses.ClaimsDocumentGenerated
                        || r.Status == SupplyRequestStatuses.MoneyRetured
                        || r.Status == SupplyRequestStatuses.ClaimsEliminated)
                            && r.DeliveredByUserId == userId)
                        .ToListAsync();
                }
                else
                {
                    _logger.LogInformation("Get requests of the User with id: {id}", userId);
                    requests = await _context.SupplyRequests
                        .Include(r => r.CreatedBy)
                        .Include(r => r.ApprovedBy)
                        .Include(r => r.DeliveredBy)
                        .Where(r => r.CreatedBy.Id == userId)
                        .ToListAsync();
                }

                return View(requests);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the retrieval of the supply request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving supply request. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/Details/5
        [HttpGet]
        [Authorize(Roles = "User,Manager,Courier")]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                // Check if id is null
                if (id == null)
                {
                    _logger.LogError("Supply request id is null.");
                    throw new ArgumentNullException(nameof(id));
                }

                // Fetch the supply request along with related entities from the database
                var request = await _context.SupplyRequests
                    .Include(r => r.CreatedBy)
                    .Include(r => r.ApprovedBy)
                    .Include(r => r.DeliveredBy)
                    .Include(r => r.RequestItems)
                    .FirstOrDefaultAsync(r => r.Id == id);

                // Check if the request is null
                if (request == null)
                {
                    _logger.LogError("Supply request with id {id} not found.", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Fetch request items related to the supply request from the database
                var requestItems = await (from i in _context.Items
                                          join ir in _context.ItemSupplyRequests on i.Id equals ir.ItemId
                                          where ir.SupplyRequestId == id
                                          select new SelectListItem()
                                          {
                                              Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                                              Value = i.Id.ToString()
                                          }).ToListAsync();

                // Set requestItems in ViewData to use in the view
                ViewData["requestItems"] = requestItems;

                // Log information about the successful retrieval of supply request
                _logger.LogInformation("Supply request details successfully retrieved for id {id}.", id);

                // Return the view with the populated supply request
                return View(request);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the details request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the details request. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/Create
        [Authorize(Policy = "User")]
        public async Task<IActionResult> Create()
        {
            // Initialize the view model
            var model = new SupplyRequestViewModel();

            try
            {
                // Fetch items with their vendors from the database
                var items = await _context.Items.Include(i => i.Vendor).ToListAsync();

                // Check if items were retrieved successfully
                if (items != null && items.Any())
                {
                    // Populate the selected items in the view model
                    model.SelectedItems = items.Select(i => new SelectListItem
                    {
                        Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                        Value = i.Id.ToString()
                    }).ToList();
                }
                else
                {
                    // Log a warning if no items were found
                    _logger.LogWarning("No items found in the database.");
                }

                // Log information about the successful retrieval of items
                _logger.LogInformation("Successfully retrieved {ItemCount} items from the database.", items?.Count ?? 0);

                // Return the populated view model to the corresponding view
                return View(model);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during item retrieval
                _logger.LogError(ex, "An error occurred while displaying create view for supply request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while displaying create view for supply request. {0}", ex.Message)));
            }
        }

        // POST: SupplyRequestControlller/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> Create(SupplyRequestViewModel model)
        {
            try
            {
                // Initialize supply request and item supply requests
                SupplyRequest request = new SupplyRequest();
                List<ItemSupplyRequest> requestItems = new List<ItemSupplyRequest>();

                // Get the current user
                var (userId, foundUser) = await this._userHelper.GetCurrentUserAsync(User).ConfigureAwait(false);

                // Set supply request properties
                request.Status = SupplyRequestStatuses.Created;
                request.CreatedAt = DateTime.UtcNow;
                request.CreatedBy = foundUser;
                request.CreatedByUserId = userId;

                // Check if model items ids are not null and not empty
                if (model.ItemsIds != null && model.ItemsIds.Length > 0)
                {
                    // Iterate through model items ids and add them to request items
                    foreach (var itemId in model.ItemsIds)
                    {
                        requestItems.Add(new ItemSupplyRequest { ItemId = itemId, SupplyRequestId = model.Id });
                    }
                    request.RequestItems = requestItems;
                }
                else
                {
                    // If model items ids are null or empty, add model state error and return to view
                    ModelState.AddModelError(string.Empty, "Choose at least one item to deliver");

                    model = new SupplyRequestViewModel();
                    model.SelectedItems = await _context.Items.Include(i => i.Vendor)
                        .Select(i => new SelectListItem
                        {
                            Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                            Value = i.Id.ToString()
                        }).ToListAsync();

                    return View(model);
                }

                // Add supply request to context and save changes
                _context.SupplyRequests.Add(request);
                _context.SaveChanges();

                // Log information about successful creation of supply request
                _logger.LogInformation("Supply request created successfully by user with id {UserId}.", userId);

                // Redirect to index action
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the creation of the supply request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the creation of the supply request. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/Edit/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                // Check if id is null or supply requests context is null
                if (id == null || _context.SupplyRequests == null)
                {
                    // Log an error if id is null or supply requests context is null
                    _logger.LogError("Supply request id is null or supply requests context is null.");
                    throw new ArgumentNullException(nameof(id));
                }

                // Initialize a view model and a list to store item ids
                SupplyRequestViewModel model = new();
                List<int> itemsIds = [];

                // Get the supply request from the context
                var request = _context.SupplyRequests.Include(r => r.RequestItems).FirstOrDefault(x => x.Id == id.Value);

                // Check if the supply request is null
                if (request == null)
                {
                    // Log an error if the supply request is null
                    _logger.LogError("Supply request with id {RequestId} not found.", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Check if the status of the supply request is not 'Created'
                if (request.Status != SupplyRequestStatuses.Created)
                {
                    // Log an error if the status of the supply request is not 'Created'
                    _logger.LogError("Cannot edit supply request with id {RequestId} because its status is not 'Created'.", id);
                    throw new InvalidDataException("Cannot edit supply request with id because its status is not 'Created'.");

                }

                // Get the item ids associated with the supply request and add them to the itemsIds list
                request.RequestItems.ToList().ForEach(i => itemsIds.Add(i.Id));

                // Bind the model with selected items
                model.SelectedItems = await _context.Items.Include(i => i.Vendor)
                    .Select(i => new SelectListItem
                    {
                        Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                        Value = i.Id.ToString()
                    }).ToListAsync();
                model.Id = request.Id;
                model.ItemsIds = itemsIds.ToArray();
                model.Status = request.Status;

                // Log information about successful retrieval and bind of supply request for editing
                _logger.LogInformation("Successfully retrieved supply request with id {RequestId} for editing.", id);

                // Return the view with the populated model
                return View(model);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the edit request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the edit request. {0}", ex.Message)));
            }
        }

        // POST: SupplyRequestControlller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Edit(int id, SupplyRequestViewModel model)
        {
            try
            {
                // Check if the id in the route does not match the id in the model
                if (id != model.Id)
                {
                    // Log an error if the ids do not match
                    _logger.LogError("Ids do not match: id in route {RouteId}, id in model {ModelId}.", id, model.Id);
                    throw new ArgumentException("Incorrect request id.");
                }

                // Check if the status of the model is not 'Created'
                if (model.Status != SupplyRequestStatuses.Created)
                {
                    // Log an error if the status is not 'Created'
                    _logger.LogError("Cannot edit supply request with id {RequestId} because its status is not 'Created'.", id);
                    throw new InvalidDataException("Cannot edit supply request with id because its status is not 'Created'.");
                }

                // Initialize a list to store item supply requests
                List<ItemSupplyRequest> requestItems = new List<ItemSupplyRequest>();

                // Get the supply request from the context
                var request = await _context.SupplyRequests.Include(r => r.RequestItems).FirstOrDefaultAsync(x => x.Id == model.Id);

                // Check if the supply request is null
                if (request == null)
                {
                    // Log an error if the supply request is null
                    _logger.LogError("Supply request with id {RequestId} not found.", model.Id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Remove existing request items from the context
                request.RequestItems.ToList().ForEach(i => requestItems.Add(i));
                _context.ItemSupplyRequests.RemoveRange(requestItems);

                // Update request details
                if (model.ItemsIds != null && model.ItemsIds.Length > 0)
                {
                    // Initialize a new list for request items
                    requestItems = new List<ItemSupplyRequest>();

                    // Iterate through model item ids and add them to request items
                    foreach (var itemId in model.ItemsIds)
                    {
                        requestItems.Add(new ItemSupplyRequest { ItemId = itemId, SupplyRequestId = model.Id });
                    }
                    request.RequestItems = requestItems;
                }
                else
                {
                    // If model item ids are null or empty, add model state error and return to view
                    ModelState.AddModelError(string.Empty, "Choose at least one item to deliver");

                    model = new SupplyRequestViewModel();
                    model.SelectedItems = await _context.Items.Include(i => i.Vendor)
                        .Select(i => new SelectListItem
                        {
                            Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                            Value = i.Id.ToString()
                        }).ToListAsync();

                    return View(model);
                }

                // Save changes to the context
                await _context.SaveChangesAsync();

                // Log information about successful editing of the supply request
                _logger.LogInformation("Supply request with id {RequestId} successfully edited.", model.Id);

                // Redirect to index action
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the edit request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the edit request. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/Approve/5
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> Approve(int? id)
        {
            try
            {
                // Check if id is null or supply requests context is null
                if (id == null || _context.SupplyRequests == null)
                {
                    // Log an error if id is null or supply requests context is null
                    _logger.LogError("Supply request id is null or supply requests context is null.");
                    throw new ArgumentNullException(nameof(id));
                }

                // Initialize a view model and a list to store item ids
                SupplyRequestViewModel model = new();
                List<int> itemsIds = [];

                // Get the supply request from the context
                var request = _context.SupplyRequests.Include(r => r.RequestItems).FirstOrDefault(x => x.Id == id.Value);

                // Check if the supply request is null
                if (request == null)
                {
                    // Log an error if the supply request is null
                    _logger.LogError("Supply request with id {RequestId} not found.", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Get the item ids associated with the supply request and add them to the itemsIds list
                request.RequestItems.ToList().ForEach(i => itemsIds.Add(i.Id));

                // Bind model with selected items
                var requestItems = await (from i in _context.Items
                                          join ir in _context.ItemSupplyRequests on i.Id equals ir.ItemId
                                          where ir.SupplyRequestId == id
                                          select new SelectListItem()
                                          {
                                              Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                                              Value = i.Id.ToString()
                                          }).ToListAsync();

                model.SelectedItems = requestItems;
                model.Id = request.Id;
                model.ItemsIds = itemsIds.ToArray();

                // Prepare supply request statuses for dropdown
                var requestStatuses = from SupplyRequestStatuses status in Enum.GetValues(typeof(SupplyRequestStatuses))
                                      where status == SupplyRequestStatuses.Approved || status == SupplyRequestStatuses.Rejected
                                      select new { Id = (int)status, Name = status.ToString() };
                ViewData["requestStatuses"] = new SelectList(requestStatuses, "Id", "Name");

                // Return the view with the populated model
                return View(model);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the approval request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the approval request. {0}", ex.Message)));
            }
        }

        // POST: SupplyRequestControlller/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> Approve(int id, SupplyRequestViewModel model)
        {
            try
            {
                // Check if the id in the route does not match the id in the model
                if (id != model.Id)
                {
                    // Log an error if the ids do not match
                    _logger.LogError("Ids do not match: id in route {RouteId}, id in model {ModelId}.", id, model.Id);
                    throw new ArgumentException("Incorrect request id.");
                }

                // Check if the status of the model is not 'Approved' or 'Rejected'
                if (model.Status != SupplyRequestStatuses.Approved && model.Status != SupplyRequestStatuses.Rejected)
                {
                    // Log an error if the status is not 'Approved' or 'Rejected'
                    _logger.LogError("Invalid status {Status} provided for approval.", model.Status);
                    ModelState.AddModelError(string.Empty, "Invalid status 'Status' provided for approval.");
                    return View(model);
                }

                // Get the supply request from the context
                var request = await _context.SupplyRequests.Include(r => r.CreatedBy).FirstOrDefaultAsync(x => x.Id == model.Id);

                // Check if the supply request is null
                if (request == null)
                {
                    // Log an error if the supply request is null
                    _logger.LogError("Supply request with id {RequestId} not found.", model.Id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Get the current user
                var (userId, foundUser) = await this._userHelper.GetCurrentUserAsync(User).ConfigureAwait(false);

                // Update supply request details
                request.ApprovedBy = foundUser;
                request.ApprovedByUserId = userId;
                request.Status = model.Status;

                // Update the supply request in the context
                _context.SupplyRequests.Update(request);
                await _context.SaveChangesAsync();

                if (request.Status == SupplyRequestStatuses.Approved)
                {
                    // Send approved notification to the user
                    await _hubContext.Clients.User(request.CreatedBy.Id).SendAsync("ReceiveNotification", String.Format("Your request number {0} has been approved.", id));
                }
                else
                {
                    // Send rejected notification to the user
                    await _hubContext.Clients.User(request.CreatedBy.Id).SendAsync("ReceiveNotification", String.Format("Your request number {0} has been rejected.", id));
                }

                // Log information about successful approval of the supply request
                _logger.LogInformation("Supply request with id {RequestId} successfully approved/rejected.", model.Id);

                // Redirect to index action
                return RedirectToAction(nameof(ManagerRequestsToApprove));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the approval request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the approval request. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/ManagerRequestsToApprove
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> ManagerRequestsToApprove()
        {
            try
            {
                // Check if the supply requests context is null
                if (_context.SupplyRequests == null)
                {
                    // Log an error if the supply requests context is null
                    _logger.LogError("Supply requests context is null.");
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.SupplyRequests' is null");
                }

                // Initialize a list to store supply requests
                List<SupplyRequest> requests = [];

                // Retrieve supply requests to be approved by the current user
                _logger.LogInformation("Get requests to approve for Manager.");
                requests = await _context.SupplyRequests
                    .Include(r => r.CreatedBy)
                    .Include(r => r.ApprovedBy)
                    .Include(r => r.DeliveredBy)
                    .Where(r => r.Status == SupplyRequestStatuses.Created)
                    .ToListAsync();

                // Return the view with the populated list of supply requests
                return View(requests);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the manager requests.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the manager requests. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/ResolveClaims/5
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> ResolveClaims(int? id)
        {
            try
            {
                // Check if the id is null or the supply requests context is null
                if (id == null || _context.SupplyRequests == null)
                {
                    // Log an error if the id is null or the supply requests context is null
                    _logger.LogError("Id is null or SupplyRequests context is null.");
                    throw new ArgumentNullException(nameof(id));
                }

                // Log information about attempting to resolve claims for a specific request
                _logger.LogInformation("Attempting to resolve claims for SupplyRequest with ID: {Id}.", id);

                // Initialize view model and list for item ids
                SupplyRequestViewModel model = new();
                List<int> itemsIds = [];

                // Get the supply request
                var request = _context.SupplyRequests.Include(r => r.RequestItems).FirstOrDefault(x => x.Id == id.Value);

                // Check if the request is null
                if (request == null)
                {
                    // Log an error if the request is null
                    _logger.LogError("Supply request with ID {Id} not found.", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Populate the item ids list
                request.RequestItems.ToList().ForEach(i => itemsIds.Add(i.Id));

                // Retrieve items related to the supply request and create select list items
                var requestItems = await (from i in _context.Items
                                          join ir in _context.ItemSupplyRequests on i.Id equals ir.ItemId
                                          where ir.SupplyRequestId == id
                                          select new SelectListItem()
                                          {
                                              Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                                              Value = i.Id.ToString()
                                          }).ToListAsync();

                // Populate the view model
                model.SelectedItems = requestItems;
                model.Id = request.Id;
                model.ClaimsText = request.ClaimsText;
                model.ItemsIds = itemsIds.ToArray();

                // Define select list items for request statuses related to claims resolution
                var requestStatuses = from SupplyRequestStatuses status in Enum.GetValues(typeof(SupplyRequestStatuses))
                                      where status == SupplyRequestStatuses.MoneyRetured
                                       || status == SupplyRequestStatuses.ClaimsEliminated
                                      select new { Id = (int)status, Name = status.ToString() };
                ViewData["claimsRequestStatuses"] = new SelectList(requestStatuses, "Id", "Name");

                // Log information about successfully loading the claims resolution view
                _logger.LogInformation("Claims resolution view loaded successfully for SupplyRequest with ID: {Id}.", id);

                // Return the view with the populated model
                return View(model);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the claims resolution request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the claims resolution request. {0}", ex.Message)));
            }
        }

        // POST: SupplyRequestControlller/ResolveClaims/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> ResolveClaims(int id, SupplyRequestViewModel model)
        {
            try
            {
                // Check if the id in the model matches the supplied id
                if (id != model.Id)
                {
                    // Log an error if the ids do not match
                    _logger.LogError("ID in the model does not match the supplied ID.");
                    throw new ArgumentException("Incorrect request id.");
                }

                // Check if the model's status is a valid claims resolution status
                if (model.Status != SupplyRequestStatuses.MoneyRetured
                    && model.Status != SupplyRequestStatuses.ClaimsEliminated)
                {
                    // Log an error if the status is not valid
                    _logger.LogError("Invalid status for claims resolution: {Status}.", model.Status);
                    ModelState.AddModelError(string.Empty, "Invalid status 'Status' provided for claims resolution.");
                    return View(model);
                }

                // Retrieve the supply request from the database
                var request = await _context.SupplyRequests.FirstOrDefaultAsync(x => x.Id == model.Id);

                // Check if the request is null
                if (request == null)
                {
                    // Log an error if the request is null
                    _logger.LogError("Supply request with ID {Id} not found.", model.Id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Update the status of the supply request
                request.Status = model.Status;

                // Update the closed at timestamp if the request is closed
                if (model.Status == SupplyRequestStatuses.MoneyRetured
                    || model.Status == SupplyRequestStatuses.ClaimsEliminated)
                {
                    request.ClosedAt = DateTime.UtcNow;
                }

                // Update the request in the database
                _context.SupplyRequests.Update(request);
                await _context.SaveChangesAsync();

                // Log information about the successful resolution of claims
                _logger.LogInformation("Claims resolved successfully for supply request with ID: {Id}.", model.Id);

                // Redirect to the manager requests page
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while resolving claims for supply request with ID: {Id}.", model.Id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while resolving claims for supply request with ID. {0}", ex.Message)));
            }
        }


        // GET: Requests To Deliver
        [HttpGet]
        [Authorize(Policy = "Courier")]
        public async Task<IActionResult> RequestsToDeliver()
        {
            try
            {
                // Check if the supply requests context is null
                if (_context.SupplyRequests == null)
                {
                    // Log an error if the supply requests context is null
                    _logger.LogError("SupplyRequests context is null.");
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.SupplyRequests' is null");
                }

                // Retrieve supply requests that are ready for delivery and have not been assigned to a courier
                _logger.LogInformation("Retrieve supply requests that are ready for delivery and have not been assigned to a courier.");
                List<SupplyRequest> requests = await _context.SupplyRequests
                .Include(r => r.CreatedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.DeliveredBy)
                .Where(r => r.Status == SupplyRequestStatuses.DelailsDocumentGenerated
                    && r.DeliveredBy == null)
                .ToListAsync();

                return View(requests);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while retrieving supply requests for delivery.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while retrieving supply requests for delivery. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/Deliver/5
        [Authorize(Policy = "Courier")]
        public async Task<IActionResult> Deliver(int? id)
        {
            try
            {
                // Check if the id is null or if the supply requests context is null
                if (id == null || _context.SupplyRequests == null)
                {
                    // Log an error if the id is null or if the supply requests context is null
                    _logger.LogError("ID or SupplyRequests context is null.");
                    throw new ArgumentNullException(nameof(id));
                }

                // Initialize a new instance of SupplyRequestViewModel
                SupplyRequestViewModel model = new();
                List<int> itemsIds = [];

                // Get the supply request from the database including its request items
                var request = _context.SupplyRequests.Include(r => r.RequestItems).FirstOrDefault(x => x.Id == id.Value);

                // Check if the request is null
                if (request == null)
                {
                    // Log an error if the request is null
                    _logger.LogError("Supply request with ID {Id} not found.", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Populate the itemsIds list with the IDs of the request items
                request.RequestItems.ToList().ForEach(i => itemsIds.Add(i.Id));

                // Retrieve the items associated with the request
                var requestItems = await (from i in _context.Items
                                          join ir in _context.ItemSupplyRequests on i.Id equals ir.ItemId
                                          where ir.SupplyRequestId == id
                                          select new SelectListItem()
                                          {
                                              Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                                              Value = i.Id.ToString()
                                          }).ToListAsync();

                // Bind the model with the request items and other necessary data
                model.SelectedItems = requestItems;
                model.Id = request.Id;
                model.ItemsIds = itemsIds.ToArray();

                // Select the delivery request statuses
                var requestStatuses = from SupplyRequestStatuses status in Enum.GetValues(typeof(SupplyRequestStatuses))
                                      where status == SupplyRequestStatuses.PendingDelivery
                                      select new { Id = (int)status, Name = status.ToString() };

                // Add the delivery request statuses to ViewData
                ViewData["deliveryRequestStatuses"] = new SelectList(requestStatuses, "Id", "Name");

                // Log information about the delivery request being processed
                _logger.LogInformation("Delivering supply request with ID {Id}.", id);

                // Return the view with the populated model
                return View(model);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the pick up for delivery request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the pick up for delivery request. {0}", ex.Message)));
            }
        }

        // POST: SupplyRequestControlller/Deliver/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Courier")]
        public async Task<IActionResult> Deliver(int id, SupplyRequestViewModel model)
        {
            try
            {
                // Check if the supplied id matches the model id
                if (id != model.Id)
                {
                    // Log an error if ids do not match
                    _logger.LogError("Supplied id does not match model id. Supplied id: {Id}, Model id: {ModelId}", id, model.Id);
                    throw new ArgumentException("Incorrect request id.");
                }

                // Check if the model status is valid for delivery
                if (model.Status != SupplyRequestStatuses.PendingDelivery)
                {
                    // Log an error if the model status is invalid
                    _logger.LogError("Invalid status for delivery: {Status}", model.Status);
                    ModelState.AddModelError(string.Empty, "Invalid status 'Status' provided for delivery.");
                    return View(model);
                }

                // Find the supply request by id
                var request = await _context.SupplyRequests.Include(r => r.CreatedBy).FirstOrDefaultAsync(x => x.Id == model.Id).ConfigureAwait(false);

                // Check if the request is not found
                if (request == null)
                {
                    // Log an error if the request is not found
                    _logger.LogError("Supply request not found for id: {0}", model.Id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Get the current user and user id
                var (userId, foundUser) = await _userHelper.GetCurrentUserAsync(User).ConfigureAwait(false);

                // Update the delivery details
                request.DeliveredBy = foundUser;
                request.DeliveredByUserId = userId;
                request.Status = model.Status;

                // Update the supply request in the database
                _context.SupplyRequests.Update(request);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                if (model.Status == SupplyRequestStatuses.PendingDelivery)
                {
                    // Send approved notification to the user
                    await _hubContext.Clients.User(request.CreatedBy.Id).SendAsync("ReceiveNotification", String.Format("Your request number {0} has been picked up for delivery.", id));
                }

                // Log successful delivery
                _logger.LogInformation("Supply request with id {Id} has been picked up for delivery successfully.", id);

                return RedirectToAction(nameof(RequestsToDeliver));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while picking up supply request for delivery with id: {id}", id);

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while picking up supply request for delivery. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/Delivered/5
        [Authorize(Policy = "Courier")]
        public async Task<IActionResult> Delivered(int? id)
        {
            try
            {
                // Check if the id is null or if supply requests context is null
                if (id == null || _context.SupplyRequests == null)
                {
                    // Log an error if the id is null or supply requests context is null
                    _logger.LogError("Id is null or SupplyRequests context is null.");
                    throw new ArgumentNullException(nameof(id));
                }

                // Initialize a new SupplyRequestViewModel and a list to store item ids
                SupplyRequestViewModel model = new SupplyRequestViewModel();
                List<int> itemsIds = new List<int>();

                // Get the supply request by id
                var request = _context.SupplyRequests.Include(r => r.RequestItems).FirstOrDefault(x => x.Id == id.Value);

                // Check if the request is null
                if (request == null)
                {
                    // Log an error if the request is null
                    _logger.LogError("Supply request not found for id: {id}", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Populate item ids for the supply request
                request.RequestItems.ToList().ForEach(i => itemsIds.Add(i.Id));

                // Bind model with selected items and other details
                var requestItems = await (from i in _context.Items
                                          join ir in _context.ItemSupplyRequests on i.Id equals ir.ItemId
                                          where ir.SupplyRequestId == id
                                          select new SelectListItem()
                                          {
                                              Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                                              Value = i.Id.ToString()
                                          }).ToListAsync();

                model.SelectedItems = requestItems;
                model.Id = request.Id;
                model.ItemsIds = itemsIds.ToArray();

                // Define delivery request statuses
                var requestStatuses = from SupplyRequestStatuses status in Enum.GetValues(typeof(SupplyRequestStatuses))
                                      where status == SupplyRequestStatuses.Delivered
                                      || status == SupplyRequestStatuses.DeliveredWithClaims
                                      select new { Id = (int)status, Name = status.ToString() };

                // Set the delivery request statuses in ViewData
                ViewData["deliveredRequestStatuses"] = new SelectList(requestStatuses, "Id", "Name");

                // Log information about successful delivery request processing
                _logger.LogInformation("Delivered request with ID {Id} processed successfully.", id);

                return View(model);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while displaying the delivered page of the request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while displaying the delivered page of the request. {0}", ex.Message)));
            }
        }

        // POST: SupplyRequestControlller/Delivered/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Courier")]
        public async Task<IActionResult> Delivered(int id, SupplyRequestViewModel model)
        {
            try
            {
                // Check if the id in the model matches the id parameter
                if (id != model.Id)
                {
                    // Log an error if the ids do not match
                    _logger.LogError("Id in model does not match the id parameter.");
                    throw new ArgumentException("Incorrect request id.");
                }

                // Check if the status in the model is valid for delivery
                if (model.Status != SupplyRequestStatuses.Delivered
                    && model.Status != SupplyRequestStatuses.DeliveredWithClaims)
                {
                    // Log an error if the status is not valid for delivery
                    _logger.LogError("Invalid status for delivery: {Status}", model.Status);
                    ModelState.AddModelError(string.Empty, "Invalid status 'Status' provided for delivery.");
                    return View(model);
                }

                // Find the supply request by id
                var request = await _context.SupplyRequests.Include(r => r.CreatedBy).FirstOrDefaultAsync(x => x.Id == model.Id);

                // Check if the request is null
                if (request == null)
                {
                    // Log an error if the request is not found
                    _logger.LogError("Supply request not found for id: {id}", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                // Update the status and additional details of the supply request
                request.Status = model.Status;
                if (model.Status == SupplyRequestStatuses.Delivered)
                {
                    request.ClosedAt = DateTime.UtcNow;
                }
                else
                {
                    request.ClaimsText = model.ClaimsText;
                }

                // Update the supply request in the database
                _context.SupplyRequests.Update(request);
                await _context.SaveChangesAsync();

                if (model.Status == SupplyRequestStatuses.Delivered)
                {
                    // Send approved notification to the user
                    await _hubContext.Clients.User(request.CreatedBy.Id).SendAsync("ReceiveNotification", String.Format("Your request number {0} has been delivered.", id));
                }
                else
                {
                    // Send rejected notification to the user
                    await _hubContext.Clients.User(request.CreatedBy.Id).SendAsync("ReceiveNotification", String.Format("Your request number {0} has been delivered with claims.", id));
                }

                // Log information about successful delivery
                _logger.LogInformation("Supply request {Id} delivered successfully.", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the delivery status of the request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the delivery status of the request. {0}", ex.Message)));
            }
        }

        // GET: SupplyRequestControlller/Delete/5
        [Authorize(Roles = "User,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                // Check if the id is null or if the supply requests context is null
                if (id == null || _context.SupplyRequests == null)
                {
                    // Log an error if the id is null or if the supply requests context is null
                    _logger.LogError("Id is null or SupplyRequests context is null.");
                    throw new ArgumentNullException(nameof(id));
                }

                // Retrieve the supply request including related entities
                var request = await _context.SupplyRequests
                    .Include(r => r.CreatedBy)
                    .Include(r => r.ApprovedBy)
                    .Include(r => r.DeliveredBy)
                    .Include(r => r.RequestItems)
                    .FirstOrDefaultAsync(r => r.Id == id);

                // Retrieve the request items associated with the supply request
                var requestItems = await (from i in _context.Items
                                          join ir in _context.ItemSupplyRequests on i.Id equals ir.ItemId
                                          where ir.SupplyRequestId == id
                                          select new SelectListItem()
                                          {
                                              Text = String.Format("Item: {0}; Vendor {1}", i.Name, i.Vendor.Name),
                                              Value = i.Id.ToString()
                                          }).ToListAsync();

                // Pass the request items to the view data
                ViewData["requestItems"] = requestItems;

                // Check if the request is null
                if (request == null)
                {
                    // Log an error if the request is null
                    _logger.LogError("Supply request not found for id: {id}", id);
                    throw new KeyNotFoundException(String.Format("Supply request not found with id: {0}", id));
                }

                return View(request);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the delete request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the delete request. {0}", ex.Message)));
            }
        }

        // POST: SupplyRequestControlller/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Log information about the delete confirmed action
                _logger.LogInformation("Deleting supply request with ID: {id}", id);

                // Check if the supply requests context is null
                if (_context.SupplyRequests == null)
                {
                    // Log an error if the supply requests context is null
                    _logger.LogError("SupplyRequests context is null.");
                    throw new SqlNullValueException("Entity set 'IApplicationDbContext.SupplyRequests' is null");
                }

                // Retrieve the supply request including related entities
                var request = await _context.SupplyRequests.Include(r => r.RequestItems).FirstOrDefaultAsync(r => r.Id == id);

                if (request != null)
                {
                    // Create a list to store request items
                    List<ItemSupplyRequest> requestItems = [];

                    // Add request items to the list
                    request.RequestItems.ToList().ForEach(i => requestItems.Add(i));

                    // Remove request items from the context
                    _context.ItemSupplyRequests.RemoveRange(requestItems);

                    // Remove the supply request from the context
                    _context.SupplyRequests.Remove(request);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Log information about successful deletion
                _logger.LogInformation("Supply request with ID: {Id} has been successfully deleted.", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, "An error occurred while processing the delete confirmed request.");

                // Handle the exception gracefully, perhaps redirecting to an error page
                // or displaying a friendly error message to the user
                return View("Error", new ErrorViewModel(String.Format("An error occurred while processing the delete confirmed request. {0}", ex.Message)));
            }
        }
    }
}
