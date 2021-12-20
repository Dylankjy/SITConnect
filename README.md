# SIT Connect - Application Security ICA
This repository contains the source code for submission as stated in the title.

### Legal Disclaimer
It is **strictly prohibited** to duplicate, use part or in whole, any piece of code in this repository without proper attribution as per AGPL-3.0 license. A copy may be found in `LICENSE` at the root of this repository.
Additionally, resubmission of this repository as academic work or claiming my work as your own is prohibited and unethical and I will exercise my full legal right against people who violate this.

Copyright (c) 2021, Dylan Kok.

All Rights Reserved.

## Registration form
**Set Strong password**
- Password complexity checks present.
    - Server-side checks will reject if requirements are not met.
    - Client-side does not allow form submission unless form is fully filled.
- Client-side provides hints on how to improve password.

**Securing user data and passwords**
- Password is hashed using Bcrypt algorithm
    - Superior as compared to SHA512 since SHA512 can hash faster than Bcrypt, allowing for faster brute forces
    - Bcrypt has in-built salting. There is no need to salt the password before hashing.
    - Implemented salting rounds is 12. Which is 2 rounds more than the industry standard.
- Card number is encrypted using AES256
    - Initialisation Vector and Key is stored on the server in `/CryptoStore/` folder. This means that the deployment server also has to be compromised for to obtain IV and keys.
    - Each key is a 512 characters, randomly generated string, hashed using SHA256 before being used in the encryption process.

**Session**
- Sessions are not generated until login has occurred. (Standard ASP.Net Core behaviour)
- Session times-out after 3 minutes of idling
- User is redirected to `/MyAccount` page after successful login.
- `/Login` and `/Register` pages are not accessible if the user is logged in.

**Login/Logout**
- User can login after registration
- Account will be locked ourt after 3 login failures.
    - Each wrong password attempt is treated as individual attempts
        - If the first attempt is done after 15 mins since the last 3rd attempt, the account is assumed to have 2 wrong attempts on record.
        - This means that the user will have to wait for all attempts to be out of the 15 minutes window to consider the account as 'allowed for login'.
    - Session cookie and all user data for said session is cleared upon logging out.
    - Each user can perform an audit own their own account. Page is located at `/AuditDisplay`.

**Anti-bot**
- reCaptcha enabled on `/Login` and `/Register`

**Proper Input Validation**
- No direct raw SQL queries were used. All queries are done via safe methods (etc. Linq).
- Client and server input validation present
    - Credit card number is validated against checksum.
    - Test card numbers are blocked

**Proper Error handling**
- Verbose error messages disabled.
- 403 is handled by `/403`.
- 404 is handled via status code 404. Left to browser to interpret and show error page.
- **Test cases**
    - `/MyAccount` (When not authenticated) - 403
    - `/thisPageDoesNotExist69` - 404
    - `/Logout` (When not authenticated) - 301

**Software Testing - Source code analysis**
- CodeQL by Github present.

**Advanced Features**
- Account will be unlocked if in the last 15 mins, there are no more than 3 password attempts.
- Password reuse disallowed.
- Password may be changed.
- Password age:
    - Cannot be changed after 5 mins since the last change.
    - Must be changed after 20 since the last password change.
        - Forced on the next login
- 2FA present.
    - 6 digit code is sent to the user's email address.
    - Each code expires after 5 mins.
    - User will need to relog in to get a new code.
        - Prevents resend email abuse.
    - All codes associated to a user account is deleted upon successful 2FA authentication and before each new OTP is sent.

---
以上です。
