namespace Shop.Domain.Errors;

public static class DomainErrors
{
    public static class Users 
    {
        public static Error EmailIsRequired()
            => Error.Validation("user.email.required", "Email is required");

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

        public static Error FirstNameIsRequired()
            => Error.Validation("user.first_name.required", "First name is required");

        public static Error LastNameIsRequired()
            => Error.Validation("user.last_name.required", "Last name is required");

        public static Error PatronymicIsRequired()
            => Error.Validation("user.patronymic.required", "Patronymic is required");

        public static Error FirstNameTooShort(int minLength)
            => Error.Validation("user.first_name.min_length", 
                $"First name must be at least {minLength} characters long");

        public static Error FirstNameTooLong(int maxLength)
            => Error.Validation("user.first_name.max_length", 
                $"First name must not exceed {maxLength} characters");

        public static Error LastNameTooShort(int minLength)
            => Error.Validation("user.last_name.min_length",
                $"Last name must be at least {minLength} characters long");

        public static Error LastNameTooLong(int maxLength)
            => Error.Validation("user.last_name.max_length",
                $"Last name must not exceed {maxLength} characters");

        public static Error PatronymicTooShort(int minLength)
            => Error.Validation("user.patronymic.min_length",
                $"Patronymic must be at least {minLength} characters long");

        public static Error PatronymicTooLong(int maxLength)
            => Error.Validation("user.patronymic.max_length",
                $"Patronymic must not exceed {maxLength} characters");

        public static Error FirstNameInvalidFormat()
            => Error.Validation("user.first_name.invalid_format",
                "First name must contain only Ukrainian letters");

        public static Error LastNameInvalidFormat()
            => Error.Validation("user.last_name.invalid_format",
                "Last name must contain only Ukrainian letters");

        public static Error PatronymicInvalidFormat()
            => Error.Validation("user.patronymic.invalid_format",
                "Patronymic must contain only Ukrainian letters");

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