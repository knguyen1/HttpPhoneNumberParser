# HttpPhoneNumberParser
Visits a webpage and looks for a phone number

Usage
__

Edit the regex pattern to look for the phone you need:
phoneRegexPattern = @"[0-9]{3}[-.][0-9]{3}[-.][0-9]{4}";

Base uri and the company name you're looking for.  Company name is needed to compare in the Levenshtein algorithm.
string baseUri = Settings1.Default.website;
string companyName = Settings1.Default.company;
