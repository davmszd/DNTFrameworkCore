using System;
using System.Net;
using System.Threading.Tasks;
using DNTFrameworkCore.Application.Models;
using DNTFrameworkCore.Application.Services;
using DNTFrameworkCore.Functional;
using DNTFrameworkCore.Web.Authorization;
using DNTFrameworkCore.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace DNTFrameworkCore.Web.API
{
    [Authorize]
    public abstract class
        CrudController<TCrudService, TKey, TModel> : CrudControllerBase<TKey, TModel, TModel,
            FilteredPagedQueryModel>
        where TCrudService : class, ICrudService<TKey, TModel>
        where TModel : MasterModel<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected readonly TCrudService Service;

        protected CrudController(TCrudService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        protected override Task<IPagedQueryResult<TModel>> ReadPagedListAsync(FilteredPagedQueryModel query)
        {
            return Service.ReadPagedListAsync(query ?? new FilteredPagedQueryModel());
        }

        protected override Task<Maybe<TModel>> FindAsync(TKey id)
        {
            return Service.FindAsync(id);
        }

        protected override Task<Result> EditAsync(TModel model)
        {
            return Service.EditAsync(model);
        }

        protected override Task<Result> CreateAsync(TModel model)
        {
            return Service.CreateAsync(model);
        }

        protected override Task<Result> DeleteAsync(TModel model)
        {
            return Service.DeleteAsync(model);
        }

        protected override Task<bool> ExistsAsync(TKey id)
        {
            return Service.ExistsAsync(id);
        }
    }

    [Authorize]
    public abstract class
        CrudController<TCrudService, TKey, TViewModel, TModel> : CrudControllerBase<TKey, TViewModel, TModel,
            FilteredPagedQueryModel>
        where TCrudService : class, ICrudService<TKey, TViewModel, TModel>
        where TViewModel : MasterModel<TKey>
        where TModel : MasterModel<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        protected readonly TCrudService Service;

        protected CrudController(TCrudService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        protected override Task<IPagedQueryResult<TViewModel>> ReadPagedListAsync(FilteredPagedQueryModel query)
        {
            return Service.ReadPagedListAsync(query ?? new FilteredPagedQueryModel());
        }

        protected override Task<Maybe<TModel>> FindAsync(TKey id)
        {
            return Service.FindAsync(id);
        }

        protected override Task<Result> EditAsync(TModel model)
        {
            return Service.EditAsync(model);
        }

        protected override Task<Result> CreateAsync(TModel model)
        {
            return Service.CreateAsync(model);
        }

        protected override Task<Result> DeleteAsync(TModel model)
        {
            return Service.DeleteAsync(model);
        }

        protected override Task<bool> ExistsAsync(TKey id)
        {
            return Service.ExistsAsync(id);
        }
    }

    [Authorize]
    public abstract class
        CrudController<TCrudService, TKey, TViewModel, TModel, TFilteredPagedQueryModel> :
            CrudControllerBase<TKey, TViewModel, TModel, TFilteredPagedQueryModel>
        where TCrudService : class, ICrudService<TKey, TViewModel, TModel, TFilteredPagedQueryModel>
        where TViewModel : MasterModel<TKey>
        where TModel : MasterModel<TKey>, new()
        where TFilteredPagedQueryModel : class, IFilteredPagedQueryModel, new()
        where TKey : IEquatable<TKey>
    {
        protected readonly TCrudService Service;

        protected CrudController(TCrudService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        protected override Task<IPagedQueryResult<TViewModel>> ReadPagedListAsync(TFilteredPagedQueryModel query)
        {
            return Service.ReadPagedListAsync(query ?? new TFilteredPagedQueryModel());
        }

        protected override Task<Maybe<TModel>> FindAsync(TKey id)
        {
            return Service.FindAsync(id);
        }

        protected override Task<Result> EditAsync(TModel model)
        {
            return Service.EditAsync(model);
        }

        protected override Task<Result> CreateAsync(TModel model)
        {
            return Service.CreateAsync(model);
        }

        protected override Task<Result> DeleteAsync(TModel model)
        {
            return Service.DeleteAsync(model);
        }

        protected override Task<bool> ExistsAsync(TKey id)
        {
            return Service.ExistsAsync(id);
        }
    }

    [ApiController]
    [Produces("application/json")]
    public abstract class
        CrudControllerBase<TKey, TViewModel, TModel, TFilteredPagedQueryModel> : ControllerBase
        where TViewModel : MasterModel<TKey>
        where TModel : MasterModel<TKey>, new()
        where TFilteredPagedQueryModel : class, IFilteredPagedQueryModel, new()
        where TKey : IEquatable<TKey>
    {
        private IAuthorizationService AuthorizationService =>
            HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        protected abstract string CreatePermissionName { get; }
        protected abstract string EditPermissionName { get; }
        protected abstract string ViewPermissionName { get; }
        protected abstract string DeletePermissionName { get; }

        protected abstract Task<IPagedQueryResult<TViewModel>> ReadPagedListAsync(TFilteredPagedQueryModel query);
        protected abstract Task<Maybe<TModel>> FindAsync(TKey id);
        protected abstract Task<Result> EditAsync(TModel model);
        protected abstract Task<Result> CreateAsync(TModel model);
        protected abstract Task<Result> DeleteAsync(TModel model);
        protected abstract Task<bool> ExistsAsync(TKey id);

        [HttpGet]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Get(TFilteredPagedQueryModel query)
        {
            if (!await CheckPermissionAsync(ViewPermissionName))
            {
                return Forbid();
            }

            var result = await ReadPagedListAsync(query ?? new TFilteredPagedQueryModel());

            return Ok(result);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.Forbidden)]
        public async Task<ActionResult<TModel>> Get([BindRequired] TKey id)
        {
            if (!await CheckPermissionAsync(EditPermissionName))
            {
                return Forbid();
            }

            var model = await FindAsync(id);

            return model.HasValue ? (ActionResult) Ok(model.Value) : NotFound();
        }

        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.Forbidden)]
        public async Task<ActionResult<TModel>> Post(TModel model)
        {
            if (!await CheckPermissionAsync(CreatePermissionName))
            {
                return Forbid();
            }

            var result = await CreateAsync(model);
            if (result.Succeeded)
            {
                return Created("", model);
            }

            result.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        [HttpPut("{id:long}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.Forbidden)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Put([BindRequired] TKey id, TModel model)
        {
            if (!model.Id.Equals(id))
            {
                return BadRequest();
            }

            if (!await CheckPermissionAsync(EditPermissionName))
            {
                return Forbid();
            }

            if (!await ExistsAsync(id))
            {
                return NotFound();
            }

            model.Id = id;

            var result = await EditAsync(model);
            if (result.Succeeded)
            {
                return NoContent();
            }

            result.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.Forbidden)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete([BindRequired] TKey id)
        {
            if (!await CheckPermissionAsync(DeletePermissionName))
            {
                return Forbid();
            }

            var model = await FindAsync(id);
            if (!model.HasValue)
            {
                return NotFound();
            }

            var result = await DeleteAsync(model.Value);
            if (result.Succeeded)
            {
                return NoContent();
            }

            result.AddToModelState(ModelState);
            return BadRequest(ModelState);
        }

        private async Task<bool> CheckPermissionAsync(string permissionName)
        {
            return (await AuthorizationService.AuthorizeAsync(User, BuildPolicyName(permissionName))).Succeeded;
        }

        private static string BuildPolicyName(string permission)
        {
            return PermissionAuthorizeAttribute.PolicyPrefix + permission;
        }
    }
}