permalink: papercut-vs-smtp4dev-testing-mail-sending-locally
title: Papercut vs smtp4dev - Testing Mail Sending Locally
date: 2010-07-01
tags: [.NET, Tools of the Trade]
---
Testing mail functionality in code using external mail servers has always been a hassle. If the mail manages to dodge antispam and various other mischievous services, it'll probably take several minutes to receive. On our Exchange servers it'll typically take 10-15 minutes before a locally sent mail actually arrives back in my inbox. We can do better!

[Papercut](http://papercut.codeplex.com) and [smtp4dev](http://smtp4dev.codeplex.com) are both applications that'll emulate a remote SMTP server on your local machine. Instead of actually sending the mail onwards like a normal SMTP server, they'll simply catch the mail and notify you. This means you'll receive the mail instantly without any risks of being caught in spam filters or dying of old age before receiving the mail. I've tried out both Papercut and smtp4dev and here are some thoughts on both.

For the testing I'm using this rather simple piece of code:

### **Program.cs **

```csharp
using System.Net.Mail;

namespace MailTest
{
    class Program
    {
        static void Main()
        {
            var sc = new SmtpClient();
            sc.Host = "127.0.0.1";

            var mail = new MailMessage("mark+from@improve.dk", "mark+to@improve.dk")
                        {
                            Subject = "My subject",
                            Body = "My body"
                        };

            sc.Send(mail);
        }
    }
}
```

## Papercut

papercut1_2.jpg

papercut2_2.jpg

### + Xcopy deployment

I love the fact that I can just unzip the download and run the .exe right away, no installation, no configuration.

### + Dead simple

It lacks some features, but when all you need is to quickly check the contents of a sent email, the simplicity of Papercut makes it extremely fast to use.

### + HTML view directly in interface

Unlike smtp4dev, Papercut will display HTML email contents directly in the interface without having to open the mail in Outlook (which I don't even have on my machine).

### + Forwarding option

If you need to forward the mail to a colleague you can do it right in the interface. It'll prompt you for an SMTP server to use - unfortunately it's not possible to provide authentication credentials.

### - HTML view should open links in new window

When checking out HTML emails I often want to verify that links are correct. Papercut will open them directly in the Papercut interface, making it hard to verify the address.

### - Lacks simple connection status in interface

There's no green bar/indicator for whether Papercut is actively listening of if it's been obstructed or failed to bind to the port on startup.

### - Lacks simple way to open .eml files

Both Papercut and smtp4dev save the emails as .eml files that can easily be opened by Outlook or antoher compatible mail client. In Papercut you have to find the executable folder and open the files directly though. A simple double-click function would be nice.

### - No way to quickly delete all mails

You have to delete all incoming mails one by one.

## smtp4dev

smtp4dev4_2.jpg

smtp4dev3_2.jpg

### + Inspection shows formatted headers

The inspection view lists headers in a table for easier reading.

### + Inspection shows mime parts

If your mail contains multiple mime parts, they can easily be seen separately in the inspection interface.

### + More UI options

smtp4dev enables you to delete all mails easily, has context menu options when right clicking the notification icon as well as generally more UI options than Papercut.

### + Lots more options

smtp4dev has vastly more options than Papercut. smtp4dev supports authentication, SSL and various other options, while Papercut only supports defining the listening port.

### + List of SMTP sessions

Using the sessions tab you can easily see how many SMTP sessions have occurred, as well as how many mails were sent as part of that session.

smtp4dev5_2.jpg

### + More activity on Codeplex

smtp4dev has somewhat more discussion activity as well as recent commit activity, whereas Papercut seems somewhat more abandoned.

### - Cumbersome message viewing

Viewing the actual email messages requires either opening the .eml files in a mail viewer application or opening up the inspection interface. Papercut is much faster since you can view the body directly in the list interface. A reading pane option would be nice.

### - Requires installation

An application like this is meant to be lightweight and easily fired up when required. smtp4dev requires installation and even offers you to let it start on bootup. It might be a matter of preferences, but I really like Papercuts xcopy deployment.

smtp4dev1_2.jpg

### - Autoupdate

Expanding on the point above; a tool like this is not a tool that I really need to have checking for updates automatically. I know, I can't really make this a minus, but still, KISS.

### - Crashes

While I haven't been able to make a reproduction case, smtp4dev crashed on me a couple of times, seemingly while running in the background without SMTP activity.

## Conclusion

So which is best? It depends! I use both... If all I need is to quickly check a single mails contents then I'll fire up Papercut. If I need to catch multiple emails and perhaps inspect the contents more thoroughly, I'll use smtp4dev. Generally I prefer to use Papercut if I don't need any further features.

Both projects' source code seems decent, though I've just skimmed quickly. smtp4dev generally seems more robust and also includes a limited amount of unit tests.
