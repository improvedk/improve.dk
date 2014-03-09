permalink: verifying-mailgun-webhooks
title: Verifying Mailgun Webhooks
date: 2013-09-23
tags: [.NET]
---
[Mailgun](http://www.mailgun.com/) has a very neat feature that enables you to basically convert incoming emails to a POST request to a URL of your choice, also known as a webhook. Using this, you can easily have your application respond to email events. However, as this URL/service needs to be publically available, verifying Mailgun webhooks is very important, ensuring requests actually come from Mailgun, and not someone impersonating Mailgun.

The code required for verifying Mailgun forwards is very simple and doesn't require much explanation:

```csharp
/// <summary>
/// Verifies that the signature matches the timestamp & token.
/// </summary>
/// <returns>True if the signature is valid, otherwise false.</returns>
public static bool VerifySignature(string key, int timestamp, string token, string signature)
{
	var encoding = Encoding.ASCII;
	var hmacSha256 = new HMACSHA256(encoding.GetBytes(key));
	var cleartext = encoding.GetBytes(timestamp + token);
	var hash = hmacSha256.ComputeHash(cleartext);
	var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

	return computedSignature == signature;
}
```

Use sample:

```csharp
// All these values are provided by the Mailgun request
var key = "key-x3ifab7xngqxep7923iuab251q5vhox0";
var timestamp = 1568491354;
var token = "asdoij2893dm98m2x0a9sdkf09k423cdm";
var signature = "AF038C73E912A830FFC830239ABFF";

// Verify if request is valid
bool isValid = VerifySignature(key, timestamp, token, signature);

```

As the [manual says](http://documentation.mailgun.com/user_manual.html#securing-webhooks) you simply need to calculate a SHA256 HMAC of the concatenated timestamp and token values, after which you can verify that it matches the Mailgun provided signature. The key is the private API key, retrievable from the Mailgun control panel.
