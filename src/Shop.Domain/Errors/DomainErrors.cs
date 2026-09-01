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
                "Name must contain only Ukrainian letters");

        public static Error RoleAlreadyAssigned()
            => Error.Conflict("user.role.already_assigned",
                "This role is already assigned to the user");

        public static Error PasswordAlreadySet()
            => Error.Conflict("user.password.already_set",
                "The new password is the same as the current password");

        public static Error PhoneAlreadySet()
            => Error.Conflict("user.phone.already_set",
                "The new phone number is the same as the current phone number");

        public static Error EmailAlreadySet()
            => Error.Conflict("user.email.already_set",
                "The new email address is the same as the current email address");

        public static Error FullNameAlreadySet()
            => Error.Conflict("user.full_name.already_set",
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
}