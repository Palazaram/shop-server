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
                $"Phone number must be {length} characters long");

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
                "A category with this slug already exists at this level");

        public static Error SlugCannotBeGenerated()
            => Error.Validation("category.slug.cannot_be_generated",
                "Could not generate a slug from the category name, provide it explicitly");

        public static Error NotFound()
            => Error.NotFound("category.not_found", "Category not found");
    }

    public static class Manufacturers
    {
        public static Error NameIsRequired()
            => Error.Validation("manufacturer.name.required", "Manufacturer name is required");

        public static Error NameTooLong(int maxLength)
            => Error.Validation("manufacturer.name.max_length",
                $"Manufacturer name must not exceed {maxLength} characters");

        public static Error CountryIsRequired()
            => Error.Validation("manufacturer.country.required", "Country is required");

        public static Error CountryTooLong(int maxLength)
            => Error.Validation("manufacturer.country.max_length",
                $"Country must not exceed {maxLength} characters");

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
}