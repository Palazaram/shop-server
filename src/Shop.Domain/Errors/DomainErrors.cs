namespace Shop.Domain.Errors;

public static class DomainErrors
{
    public static class Users
    {
        public static Error EmailIsRequired()
            => Error.Validation("user.email.required", "Email is required");

        public static Error EmailTooLong(int maxLength)
            => Error.Validation("user.email.max_length",
                $"Email must not exceed {maxLength} characters");

        public static Error EmailInvalidFormat()
            => Error.Validation("user.email.invalid_format", "Email has invalid format");

        public static Error PhoneIsRequired()
            => Error.Validation("user.phone.required", "Phone number is required");

        public static Error PhoneInvalidLength(int length)
            => Error.Validation("user.phone.invalid_length",
                $"Phone number must contain {length} digits after the country code");

        public static Error PhoneInvalidFormat()
            => Error.Validation("user.phone.invalid_format", "Phone number has invalid format");

        public static Error PasswordHashIsRequired()
            => Error.Validation("user.password_hash.required", "Password hash is required");

        public static Error NameIsRequired()
            => Error.Validation("user.name.required", "Name is required");

        public static Error NameTooShort(int minLength)
            => Error.Validation("user.name.min_length",
                $"Name must be at least {minLength} characters long");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("user.name.max_length",
                $"Name must not exceed {maxLength} characters");

        public static Error NameInvalidFormat()
            => Error.Validation("user.name.invalid_format",
                "Name must contain only Ukrainian letters, apostrophes and hyphens");

        public static Error RoleAlreadyAssigned()
            => Error.Conflict("user.role.already_assigned",
                "This role is already assigned to the user");

        public static Error PasswordUnchanged()
            => Error.Conflict("user.password.unchanged",
                "The new password is the same as the current password");

        public static Error PhoneUnchanged()
            => Error.Conflict("user.phone.unchanged",
                "The new phone number is the same as the current phone number");

        public static Error EmailUnchanged()
            => Error.Conflict("user.email.unchanged",
                "The new email address is the same as the current email address");

        public static Error FullNameUnchanged()
            => Error.Conflict("user.full_name.unchanged",
                "The new full name is the same as the current one");

        public static Error EmailAlreadyExists()
            => Error.Conflict("user.email.already_exists",
                "A user with this email address already exists");

        public static Error PhoneAlreadyExists()
            => Error.Conflict("user.phone.already_exists",
                "A user with this phone number already exists");

        public static Error PasswordIsRequired()
            => Error.Validation("user.password.required", "Password is required");

        public static Error PasswordTooShort(int minLength)
            => Error.Validation("user.password.min_length",
                $"Password must be at least {minLength} characters long");

        public static Error PasswordMissingUppercase()
            => Error.Validation("user.password.missing_uppercase",
                "Password must contain at least one uppercase letter");

        public static Error PasswordMissingDigit()
            => Error.Validation("user.password.missing_digit",
                "Password must contain at least one digit");
    }

    public static class RefreshTokens
    {
        public static Error TokenHashIsRequired()
            => Error.Validation("refresh_token.hash.required",
                "Token hash is required");

        public static Error AlreadyRevoked()
            => Error.Conflict("refresh_token.already_revoked",
                "Refresh token is already revoked");
    }

    public static class Auth
    {
        public static Error InvalidCredentials()
            => Error.Unauthorized("auth.invalid_credentials",
                "Invalid credentials");

        public static Error RefreshTokenInvalid()
            => Error.Unauthorized("auth.refresh_token_invalid",
                "Refresh token is invalid");
    }

    public static class Slugs
    {
        public static Error IsRequired()
            => Error.Validation("slug.required", "Slug is required");

        public static Error TooLong(int maxLength)
            => Error.Validation("slug.max_length",
                $"Slug must not exceed {maxLength} characters");

        public static Error InvalidFormat()
            => Error.Validation("slug.invalid_format",
                "Slug must contain only lowercase latin letters, digits and single hyphens");
    }

    public static class Categories
    {
        public static Error NameIsRequired()
            => Error.Validation("category.name.required", "Category name is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("category.name.max_length",
                $"Category name must not exceed {maxLength} characters");

        public static Error ParentIdIsInvalid()
            => Error.Validation("category.parent_id.invalid",
                "Parent category id must not be empty");

        public static Error ParentNotFound()
            => Error.Validation("category.parent.not_found",
                "Parent category does not exist");

        public static Error MaxDepthExceeded()
            => Error.Validation("category.max_depth_exceeded",
                "A subcategory cannot have subcategories of its own");

        public static Error NameAlreadyExists()
            => Error.Conflict("category.name.already_exists",
                "A category with this name already exists at this level");

        public static Error SlugAlreadyExists()
            => Error.Conflict("category.slug.already_exists",
                "A category with this slug already exists");


        public static Error MetaTitleTooLong(int maxLength)
            => Error.Validation("category.meta_title.max_length",
                $"Meta title must not exceed {maxLength} characters");

        public static Error MetaDescriptionTooLong(int maxLength)
            => Error.Validation("category.meta_description.max_length",
                $"Meta description must not exceed {maxLength} characters");

        /// <summary>
        /// Адрес существовал раньше и сменился. 404, а не 301: редирект посетителю отдаёт фронт,
        /// он же общается с поисковиком, — а API лишь называет новый адрес.
        /// </summary>
        public static Error SlugMoved()
            => Error.NotFound("category.slug.moved",
                "This category address has changed");

        public static Error SlugCannotBeGenerated()
            => Error.Validation("category.slug.cannot_be_generated",
                "Could not generate a slug from the category name, provide it explicitly");

        public static Error NotFound()
            => Error.NotFound("category.not_found", "Category not found");

        public static Error DuplicateAttribute()
            => Error.Validation("category.attributes.duplicate",
                "The same attribute is listed more than once");

        public static Error AttributeNotFound()
            => Error.Validation("category.attributes.not_found",
                "One of the attributes does not exist");

        public static Error AttributeIdsAreRequired()
            => Error.Validation("category.attributes.required", "Attribute ids are required");

        public static Error AttributeIdIsInvalid()
            => Error.Validation("category.attributes.invalid_id", "Attribute id must not be empty");

        public static Error OrderIsRequired()
            => Error.Validation("category.order.required", "Category ids are required");

        public static Error OrderIdIsInvalid()
            => Error.Validation("category.order.invalid_id", "Category id must not be empty");

        public static Error DuplicateCategoryInOrder()
            => Error.Validation("category.order.duplicate",
                "The same category is listed more than once");

        public static Error CategoryNotOnLevel(Guid categoryId)
            => Error.Validation("category.order.not_on_level",
                $"Category {categoryId} does not belong to this level");

        public static Error OrderIsIncomplete(int expectedCount)
            => Error.Validation("category.order.incomplete",
                $"All {expectedCount} categories of this level must be listed");
    }

    public static class Manufacturers
    {
        public static Error NameIsRequired()
            => Error.Validation("manufacturer.name.required", "Manufacturer name is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("manufacturer.name.max_length",
                $"Manufacturer name must not exceed {maxLength} characters");

        public static Error CountryIdIsInvalid()
            => Error.Validation("manufacturer.country_id.invalid",
                "Country id must not be empty");

        public static Error CountryNotFound()
            => Error.Validation("manufacturer.country.not_found", "Country does not exist");

        public static Error NotFound()
            => Error.NotFound("manufacturer.not_found", "Manufacturer not found");

        public static Error NameAlreadyExists()
            => Error.Conflict("manufacturer.name.already_exists",
                "A manufacturer with this name already exists");

        public static Error SlugAlreadyExists()
            => Error.Conflict("manufacturer.slug.already_exists",
                "A manufacturer with this slug already exists");

        public static Error SlugCannotBeGenerated()
            => Error.Validation("manufacturer.slug.cannot_be_generated",
                "Could not generate a slug from the manufacturer name, provide it explicitly");
    }

    public static class Countries
    {
        public static Error NameIsRequired()
            => Error.Validation("country.name.required", "Country name is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("country.name.max_length",
                $"Country name must not exceed {maxLength} characters");

        public static Error NameAlreadyExists()
            => Error.Conflict("country.name.already_exists",
                "A country with this name already exists");

        public static Error SlugAlreadyExists()
            => Error.Conflict("country.slug.already_exists",
                "A country with this slug already exists");

        public static Error SlugCannotBeGenerated()
            => Error.Validation("country.slug.cannot_be_generated",
                "Could not generate a slug from the country name, provide it explicitly");

        public static Error NotFound()
            => Error.NotFound("country.not_found", "Country not found");
    }

    public static class Money
    {
        public static Error MustBePositive()
            => Error.Validation("money.must_be_positive", 
                "Amount must be greater than zero");

        public static Error IsRequired()
            => Error.Validation("money.required", "Price is required");
    }

    public static class Packagings
    {
        public static Error ValueMustBePositive()
            => Error.Validation("packaging.value.must_be_positive",
                "Packaging value must be greater than zero");

        public static Error UnitIsInvalid()
            => Error.Validation("packaging.unit.invalid", "Unknown unit of measure");

        public static Error ValueIsRequired()
            => Error.Validation("packaging.value.required", "Packaging value is required");

        public static Error UnitIsRequired()
            => Error.Validation("packaging.unit.required", "Unit of measure is required");
    }

    public static class Products
    {
        public static Error NameIsRequired()
            => Error.Validation("product.name.required", "Product name is required");

        public static Error MetaTitleTooLong(int maxLength)
            => Error.Validation("product.meta_title.max_length",
                $"Meta title must not exceed {maxLength} characters");

        public static Error MetaDescriptionTooLong(int maxLength)
            => Error.Validation("product.meta_description.max_length",
                $"Meta description must not exceed {maxLength} characters");

        public static Error IsFeaturedIsRequired()
            => Error.Validation("product.is_featured.required", "Featured flag is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("product.name.max_length",
                $"Product name must not exceed {maxLength} characters");

        public static Error DescriptionTooLong(int maxLength)
            => Error.Validation("product.description.max_length",
                $"Description must not exceed {maxLength} characters");

        public static Error NotFound()
            => Error.NotFound("product.not_found", "Product not found");

        public static Error CategoryNotFound()
            => Error.Validation("product.category.not_found", "Category does not exist");

        public static Error CategoryIsNotLeaf()
            => Error.Validation("product.category.not_leaf",
                "Products can only be attached to a category without subcategories");

        public static Error ManufacturerNotFound()
            => Error.Validation("product.manufacturer.not_found", "Manufacturer does not exist");

        public static Error NameAlreadyExists()
            => Error.Conflict("product.name.already_exists",
                "This manufacturer already has a product with this name");

        public static Error CategoryIdIsInvalid()
            => Error.Validation("product.category_id.invalid", 
                "Category id must not be empty");

        public static Error ManufacturerIdIsInvalid()
            => Error.Validation("product.manufacturer_id.invalid", 
                "Manufacturer id must not be empty");

        public static Error AttributeValueIdsAreRequired()
            => Error.Validation("product.attribute_values.required", 
                "Attribute value ids are required");

        public static Error AttributeValueIdIsInvalid()
            => Error.Validation("product.attribute_values.invalid_id",
                "Attribute value id must not be empty");

        public static Error SpecificationsAreRequired()
            => Error.Validation("product.specifications.required",
                "Specifications are required");

        public static Error SpecificationIdIsInvalid()
            => Error.Validation("product.specifications.invalid_id",
                "Specification id must not be empty");

        public static Error DuplicateSpecification()
            => Error.Validation("product.specifications.duplicate",
                "The same specification is listed more than once");

        public static Error SpecificationNotFound()
            => Error.Validation("product.specifications.not_found",
                "One of the specifications does not exist");

        public static Error SpecificationValueIsRequired()
            => Error.Validation("product.specifications.value_required",
                "Specification value is required");

        public static Error SpecificationValueTooLong(int maxLength)
            => Error.Validation("product.specifications.value_max_length",
                $"Specification value must not exceed {maxLength} characters");

        public static Error DuplicateAttributeValue()
            => Error.Validation("product.attribute_values.duplicate",
                "The same attribute value is listed more than once");

        public static Error AttributeValueNotFound()
            => Error.Validation("product.attribute_values.not_found",
                "One of the attribute values does not exist");

        public static Error AttributeNotApplicable(IEnumerable<string> valueNames)
            => Error.Validation("product.attribute_values.not_applicable",
                "These values belong to attributes that are not applicable to this product's category: "
                + string.Join(", ", valueNames));

        public static Error UnknownFilter(string attributeSlug)
            => Error.Validation("product.filter.unknown_attribute",
                $"Unknown filter '{attributeSlug}'");

        public static Error UnknownFilterValue(string attributeSlug, string valueSlug)
            => Error.Validation("product.filter.unknown_value",
                $"Filter '{attributeSlug}' has no value '{valueSlug}'");

        public static Error AttributeFilterOutsideCategory(string groupKey)
            => Error.Validation("product.filter.requires_category",
                $"Filter '{groupKey}' is only available inside a category");

        public static Error UnknownSort(string sort)
            => Error.Validation("product.sort.unknown",
                $"Unknown sort order '{sort}'");

        public static Error InvalidPrice(string key, string value)
            => Error.Validation("product.filter.invalid_price",
                $"Parameter '{key}' is not a valid price: '{value}'");

        public static Error InvalidFeaturedFilter(string value)
            => Error.Validation("product.filter.invalid_featured",
                $"Parameter 'featured' must be true or false: '{value}'");

        public static Error TooManyImages(int maxImages)
            => Error.Validation("product.images.too_many",
                $"A product can have at most {maxImages} images");

        public static Error ImageNotFound()
            => Error.NotFound("product.image.not_found", "Image not found");

        public static Error ImageAltTooLong(int maxLength)
            => Error.Validation("product.image.alt.max_length",
                $"Image alt text must not exceed {maxLength} characters");

        public static Error DuplicateImageInOrder()
            => Error.Validation("product.images.duplicate",
                "The same image is listed more than once");

        public static Error ImageOrderIsIncomplete(int expectedCount)
            => Error.Validation("product.images.order_incomplete",
                $"The order must list all {expectedCount} images of the product");

        public static Error ImageIsRequired()
            => Error.Validation("product.image.required", "Image file is required");

        public static Error ImageFileTooLarge(int maxBytes)
            => Error.Validation("product.image.file_too_large",
                $"Image file must not exceed {maxBytes / (1024 * 1024)} MB");

        public static Error ImageFormatNotSupported()
            => Error.Validation("product.image.invalid_format",
                "Only JPEG, PNG and WebP images are supported");

        public static Error ImageResolutionTooLarge(int maxPixels)
            => Error.Validation("product.image.resolution_too_large",
                $"Image must not exceed {maxPixels / 1_000_000} megapixels");

        public static Error ImageOrderIsRequired()
            => Error.Validation("product.images.order_required", "Image order is required");

        public static Error ImageIdIsInvalid()
            => Error.Validation("product.images.invalid_id", "Image id must not be empty");

        public static Error ImageNotOnProduct(Guid imageId)
            => Error.Validation("product.images.unknown_image",
                $"Image {imageId} does not belong to this product");

        public static Error SearchTooShort(int minLength)
            => Error.Validation("product.search.too_short",
                $"Search query must be at least {minLength} characters long");

        public static Error SearchTooLong(int maxLength)
            => Error.Validation("product.search.too_long",
                $"Search query must not exceed {maxLength} characters");

        public static Error RelevanceSortRequiresSearch()
            => Error.Validation("product.sort.relevance_requires_search",
                "Sorting by relevance requires a search query");
    }

    public static class ProductVariants
    {
        public static Error SkuIsRequired()
            => Error.Validation("product_variant.sku.required", "SKU is required");

        public static Error SkuTooLong(int maxLength)
            => Error.Validation("product_variant.sku.max_length",
                $"SKU must not exceed {maxLength} characters");

        public static Error StockMustNotBeNegative()
            => Error.Validation("product_variant.stock.negative",
                "Stock quantity must not be negative");

        public static Error NotFound()
            => Error.NotFound("product_variant.not_found", "Product variant not found");

        public static Error ProductNotFound()
            => Error.Validation("product_variant.product.not_found", "Product does not exist");

        public static Error SkuAlreadyExists()
            => Error.Conflict("product_variant.sku.already_exists",
                "A variant with this SKU already exists");

        public static Error SlugMoved()
            => Error.NotFound("product_variant.slug.moved",
                "This product address has changed");

        public static Error SlugAlreadyExists()
            => Error.Conflict("product_variant.slug.already_exists",
                "A variant with this slug already exists");

        public static Error PackagingAlreadyExists()
            => Error.Conflict("product_variant.packaging.already_exists",
                "This product already has a variant with the same packaging");

        public static Error SlugCannotBeGenerated()
            => Error.Validation("product_variant.slug.cannot_be_generated",
                "Could not generate a slug for this variant, provide it explicitly");

        public static Error StockIsRequired()
            => Error.Validation("product_variant.stock.required", 
                "Stock quantity is required");

        public static Error GeneratedSlugAlreadyExists()
            => Error.Conflict("product_variant.slug.generated_conflict",
                "A variant with the automatically generated slug already exists, provide a slug explicitly");
    }

    public static class ProductAttributes
    {
        public static Error NameIsRequired()
            => Error.Validation("product_attribute.name.required", "Attribute name is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("product_attribute.name.max_length",
                $"Attribute name must not exceed {maxLength} characters");

        public static Error NotFound()
            => Error.NotFound("product_attribute.not_found", "Attribute not found");

        public static Error NameAlreadyExists()
            => Error.Conflict("product_attribute.name.already_exists",
                "An attribute with this name already exists");

        public static Error SlugAlreadyExists()
            => Error.Conflict("product_attribute.slug.already_exists",
                "An attribute with this slug already exists");

        public static Error SlugCannotBeGenerated()
            => Error.Validation("product_attribute.slug.cannot_be_generated",
                "Could not generate a slug from the attribute name, provide it explicitly");

        public static Error GeneratedSlugAlreadyExists()
            => Error.Conflict("product_attribute.slug.generated_conflict",
                "An attribute with the automatically generated slug already exists, provide a slug explicitly");
    }

    public static class AttributeValues
    {
        public static Error NameIsRequired()
            => Error.Validation("attribute_value.name.required", "Value name is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("attribute_value.name.max_length",
                $"Value name must not exceed {maxLength} characters");

        public static Error NotFound()
            => Error.NotFound("attribute_value.not_found", "Attribute value not found");

        public static Error AttributeNotFound()
            => Error.Validation("attribute_value.attribute.not_found", "Attribute does not exist");

        public static Error NameAlreadyExists()
            => Error.Conflict("attribute_value.name.already_exists",
                "This attribute already has a value with this name");

        public static Error SlugAlreadyExists()
            => Error.Conflict("attribute_value.slug.already_exists",
                "This attribute already has a value with this slug");

        public static Error SlugCannotBeGenerated()
            => Error.Validation("attribute_value.slug.cannot_be_generated",
                "Could not generate a slug from the value name, provide it explicitly");

        public static Error GeneratedSlugAlreadyExists()
            => Error.Conflict("attribute_value.slug.generated_conflict",
                "This attribute already has a value with the automatically generated slug, provide a slug explicitly");
    }

    public static class Specifications
    {
        public static Error NameIsRequired()
            => Error.Validation("specification.name.required", "Specification name is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("specification.name.max_length",
                $"Specification name must not exceed {maxLength} characters");

        public static Error NameAlreadyExists()
            => Error.Conflict("specification.name.already_exists",
                "A specification with this name already exists");

        public static Error NotFound()
            => Error.NotFound("specification.not_found", "Specification not found");

        public static Error OrderIsRequired()
            => Error.Validation("specification.order.required", "Specification ids are required");

        public static Error IdIsInvalid()
            => Error.Validation("specification.order.invalid_id",
                "Specification id must not be empty");

        public static Error DuplicateInOrder()
            => Error.Validation("specification.order.duplicate",
                "The same specification is listed more than once");

        /// <summary>
        /// Идентификатор приходит в теле, а не в адресе, поэтому 400, а не 404.
        /// </summary>
        public static Error UnknownIdInOrder(Guid specificationId)
            => Error.Validation("specification.order.unknown_id",
                $"Specification '{specificationId}' does not exist");

        public static Error OrderIsIncomplete(int expectedCount)
            => Error.Validation("specification.order.incomplete",
                $"The order must list all {expectedCount} specifications");
    }
}
