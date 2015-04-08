HttpPhoneNumberParser

HOW TO LAUNCH
___

This program was compiled using 4.5 Framework.
The exe is located at

	/root/bin/HttpPhoneNumberParser.exe

The executable takes two arguments, which are OPTIONAL:

	HttpPhoneNumberParser.exe [HTTP_ADDRESS] [COMPANY_NAME]

It defaults to KLS Diversified parameters.


DISCUSSION
___

The purpose of this program is to look for a phone number from a company website.  This program takes two inputs: the base uri, and the company name.  First, we will visit the homepage and use Regex to find all visiable links.  We will then find the phone number on the homepage as well as all other pages.  We use Regex with the matching pattern of:

	@"[0-9]{3}[-.][0-9]{3}[-.][0-9]{4}"

Then, we will hit the OpenCNam API to verify the numbers.  OpenCNAM is an API that lets you get the name associated with the number.  IMPORTANT: YOU CAN ONLY MAKE 10 CALLS PER HOUR.

Finally, we compare the inputed company name and the name returned by OPENCNAM using the Levenshtein Edit Distance algorithm.

We only get back numbers with a distance of less than 4 (a good match).