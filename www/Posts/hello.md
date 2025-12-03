Hello, IndieWeb!
1764777655

## At Last, Blog!
I've finally managed to finish setting up my blog. I've been meaning to create one for a while now, but never found the drive to do it. Then I found the [IndieWeb](https://indieweb.org/) movement, so I thought, "why not?".

Game development is about solving problems, and I've solved (or at least tried to solve) many over the years. Why not braindump them somewhere and provide some value to someone else along the way?

## Create What You Need
Initially my goal was to use a static site generator. Then I read [this](https://indieweb.org/make_what_you_need) and decided to whip up a simple web server myself in C#. How hard that could be?

Turns out, it is pretty straightforward. C# has its own HTTP server, so all that I had to code is some static file serving and a basic routing handler logic.

> I ain't going to code the HTTP server from scratch, no.

I wanted to author my posts in markdown so I looked around for a markdown engine in C# and came across [MarkDig](https://github.com/xoofx/markdig). After adding it to the project, I've written a simple route handler that takes the posts, extracts title from the first line, extracts the unix timestamp from the second line, then converts the markdown to html using MarkDig, and finally injects title & date into the template. Whew!

> I initially wanted to extract timestamps from the files but platform differences and gotchas were ruining it for me, so I'm manually entering timestamps for the files for now.

It's pretty barebones, but handles my usecase. Using C# is a delight, so all in all it was a fun project. I don't think I'll be adding more features to it, but who knows? One thing that would be nice to have is a file watcher that updates the post cache if any file is modified or new one is added. But restarting the server works for now.

Working with raw HTML and CSS was also an interesting experience. It wasn't *that* hard to whip up a simple design, but I get why there's a ton of tools around styling webpages.
## Conclusion
Anyway, thank you for your time. You can browse the [source code here](/). Here's to many more blog posts!

