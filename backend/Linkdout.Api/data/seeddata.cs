using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Linkdout.Api.Models;

namespace Linkdout.Api.Data;

public static class SeedData
{
    public static async Task Initialize(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return; // already seeded

        // ═══════════════ USERS ═══════════════
        var users = new List<User>
        {
            new() { FullName = "كيرلس صلاح فخري", Email = "keroles@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "AI Engineer · CS Student · Content Creator", Bio = "طالب علوم حاسب في جامعة أسيوط الأهلية، شغوف بالذكاء الاصطناعي وتطوير الويب. هدفي بناء منتجات رقمية مفيدة ومساعدة الطلاب على تطوير تفكيرهم التقني.", Location = "أسيوط، مصر", Website = "https://keroles-sala.me/", Status = "open", Skills = JsonSerializer.Serialize(new[] { "Python", "Machine Learning", "HTML & CSS", "JavaScript", "C++", "SQL", "Git", "Problem Solving", "UI/UX Design" }) },
            new() { FullName = "أحمد مصطفى", Email = "ahmed@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "Senior Frontend Developer في Vois", Bio = "Frontend developer بخبرة 5 سنين. متخصص في React و Next.js. بحب أشارك المعرفة مع الجونيورز وأساعدهم يبدأوا صح.", Location = "القاهرة، مصر", Status = "available", Skills = JsonSerializer.Serialize(new[] { "React", "Next.js", "TypeScript", "JavaScript", "HTML", "CSS", "Tailwind CSS", "Redux" }) },
            new() { FullName = "نورا علي", Email = "noura@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "Data Scientist في Sprints", Bio = "Data Scientist مهتمة بالـ NLP والـ Computer Vision. درست في كلية حاسبات ومعلومات وحالياً بشتغل على مشاريع AI في المجال الطبي.", Location = "الإسكندرية، مصر", Status = "open", Skills = JsonSerializer.Serialize(new[] { "Python", "TensorFlow", "PyTorch", "NLP", "Computer Vision", "SQL", "Data Analysis" }) },
            new() { FullName = "محمد جمال", Email = "mohamed@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "CTO & Co-Founder في TechVillage", Bio = "باني أنظمة من 10 سنين. بشتغل على Cloud Architecture و Microservices. بحب أ mentor المبتدئين وأساعد الشركات الناشئة.", Location = "القاهرة، مصر", Status = "learning", Skills = JsonSerializer.Serialize(new[] { "System Design", "AWS", "Kubernetes", "Docker", "Node.js", "Python", "PostgreSQL", "Redis" }) },
            new() { FullName = "سلمى إبراهيم", Email = "salma@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "UX Designer | Design Mentor", Bio = "مصممة UX مع 4 سنين خبرة. شغالة مع Startups مصرية وعالمية. بحب أعلم الـ design thinking للطلاب.", Location = "الجيزة، مصر", Status = "available", Skills = JsonSerializer.Serialize(new[] { "Figma", "Design Systems", "User Research", "Prototyping", "UI Design", "Design Thinking" }) },
            new() { FullName = "كريم عادل", Email = "karim@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "Full Stack Developer | Freelancer", Bio = "Full Stack Developer شغال freelance على Upwork و Mostaql. بقالي سنتين في المجال وبحب أساعد الناس اللي لسه بادئة في الفريلانس.", Location = "أسيوط، مصر", Status = "open", Skills = JsonSerializer.Serialize(new[] { "React", "Node.js", "MongoDB", "Express", "JavaScript", "TypeScript", "Git" }) },
            new() { FullName = "دينا فؤاد", Email = "dina@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "AI Researcher | PhD Candidate", Bio = "باحثة دكتوراه في جامعة القاهرة، متخصصة في Reinforcement Learning. نزلت 3 ورقات بحثية في مؤتمرات دولية.", Location = "القاهرة، مصر", Status = "learning", Skills = JsonSerializer.Serialize(new[] { "PyTorch", "Reinforcement Learning", "Deep Learning", "Python", "LaTeX", "Research" }) },
            new() { FullName = "عمر خالد", Email = "omar@linkdout.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234"), Headline = "Mobile Developer | Flutter Expert", Bio = "Mobile developer متخصص في Flutter. اشتغلت على 15+ تطبيق موبايل لشركات مصرية وسعودية. بحب الـ clean architecture.", Location = "طنطا، مصر", Status = "open", Skills = JsonSerializer.Serialize(new[] { "Flutter", "Dart", "Firebase", "iOS", "Android", "Clean Architecture" }) }
        };
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        // ═══════════════ EXPERIENCES ═══════════════
        var experiences = new List<Experience>
        {
            new() { UserId = 1, Title = "Problem Solving Mentor", Company = "ANU ICPC Community", Type = "work", StartDate = new DateTime(2025, 3, 1), IsCurrent = true, Description = "تدريب طلاب الجامعة على حل المشكلات البرمجية والخوارزميات" },
            new() { UserId = 1, Title = "Graphic Designer", Company = "Human For Good Community", Type = "volunteer", StartDate = new DateTime(2024, 4, 1), IsCurrent = true, Description = "تصميم الهوية البصرية والبوسترات للمجتمع" },
            new() { UserId = 1, Title = "Content Creator", Company = "YouTube (@kerlssalah)", Type = "work", StartDate = new DateTime(2023, 12, 1), IsCurrent = true, Description = "إنتاج محتوى تعليمي في البرمجة والتقنية" },
            new() { UserId = 2, Title = "Senior Frontend Developer", Company = "Vois", Type = "work", StartDate = new DateTime(2023, 1, 1), IsCurrent = true, Description = "تطوير واجهات المستخدم لتطبيق Vois باستخدام React و Next.js" },
            new() { UserId = 3, Title = "Data Scientist", Company = "Sprints", Type = "work", StartDate = new DateTime(2024, 6, 1), IsCurrent = true, Description = "تطوير نماذج ML لتحليل البيانات الطبية" },
            new() { UserId = 4, Title = "CTO & Co-Founder", Company = "TechVillage", Type = "work", StartDate = new DateTime(2020, 1, 1), IsCurrent = true, Description = "قيادة الفريق التقني وبناء البنية التحتية السحابية" }
        };
        db.Experiences.AddRange(experiences);

        // ═══════════════ COMPANIES ═══════════════
        var companies = new List<Company>
        {
            new() { Name = "Vois", Industry = "تكنولوجيا · تطوير برمجيات", Description = "Vois هي منصة تواصل اجتماعي صوتي مصرية. بنربط الناس من خلال المحادثات الصوتية المباشرة وبنبني مجتمعات حقيقية.", Location = "القاهرة، مصر", Size = "50-200", Website = "vois.eg", CoverColor = "pattern-1" },
            new() { Name = "Sprints", Industry = "تعليم · تكنولوجيا", Description = "Sprints منصة تعليمية مصرية بتقدم كورسات ودورات تدريبية في مجالات التكنولوجيا. هدفنا سد الفجوة بين التعليم الأكاديمي وسوق العمل.", Location = "القاهرة، مصر", Size = "50-200", Website = "sprints.ai", CoverColor = "pattern-2" },
            new() { Name = "TechVillage", Industry = "برمجيات · خدمات رقمية", Description = "TechVillage شركة برمجيات مصرية متخصصة في بناء الحلول الرقمية للشركات الناشئة. بنؤمن بالتكنولوجيا للجميع وبنركز على تدريب المواهب الشابة.", Location = "القاهرة، مصر", Size = "50-200", Website = "techvillage.eg", CoverColor = "pattern-3" },
            new() { Name = "Eureka Digital", Industry = "تسويق · تصميم · تكنولوجيا", Description = "Eureka Digital وكالة تسويق رقمي متكاملة. بنقدم خدمات تصميم، تطوير، وإدارة حملات رقمية للعلامات التجارية.", Location = "الإسكندرية، مصر", Size = "10-50", Website = "eurekadigital.eg", CoverColor = "pattern-4" },
            new() { Name = "AI Lab Egypt", Industry = "ذكاء اصطناعي · أبحاث", Description = "AI Lab Egypt معمل أبحاث مصري متخصص في الذكاء الاصطناعي. بنعمل أبحاث في NLP, Computer Vision, و Robotics.", Location = "القاهرة، مصر", Size = "10-50", Website = "ailab.eg", CoverColor = "pattern-5" }
        };
        db.Companies.AddRange(companies);
        await db.SaveChangesAsync();

        // ═══════════════ JOBS ═══════════════
        var jobs = new List<Job>
        {
            new() { CompanyId = 1, Title = "Junior Frontend Developer (React)", Type = "full-time", Location = "القاهرة", RequiredSkills = JsonSerializer.Serialize(new[] { "React", "JavaScript", "HTML", "CSS" }) },
            new() { CompanyId = 1, Title = "Backend Developer (Node.js)", Type = "full-time", Location = "القاهرة", RequiredSkills = JsonSerializer.Serialize(new[] { "Node.js", "Express", "PostgreSQL" }) },
            new() { CompanyId = 2, Title = "AI Internship Program", Type = "internship", Location = "أسيوط / عن بعد", RequiredSkills = JsonSerializer.Serialize(new[] { "Python", "ML Basics", "Problem Solving" }) },
            new() { CompanyId = 2, Title = "Data Science Trainer", Type = "part-time", Location = "القاهرة", RequiredSkills = JsonSerializer.Serialize(new[] { "Python", "ML", "Teaching" }) },
            new() { CompanyId = 3, Title = "Python Backend Developer", Type = "full-time", Location = "عن بعد", RequiredSkills = JsonSerializer.Serialize(new[] { "Python", "Django", "PostgreSQL" }) },
            new() { CompanyId = 3, Title = "AI/ML Intern", Type = "internship", Location = "القاهرة", RequiredSkills = JsonSerializer.Serialize(new[] { "Python", "TensorFlow", "ML" }) },
            new() { CompanyId = 3, Title = "DevOps Engineer (Entry Level)", Type = "full-time", Location = "عن بعد", RequiredSkills = JsonSerializer.Serialize(new[] { "Docker", "AWS", "CI/CD" }) },
            new() { CompanyId = 4, Title = "UI/UX Designer (Fresh Grad)", Type = "full-time", Location = "الإسكندرية", RequiredSkills = JsonSerializer.Serialize(new[] { "Figma", "Design Systems" }) },
            new() { CompanyId = 5, Title = "Research Assistant - NLP", Type = "full-time", Location = "القاهرة", RequiredSkills = JsonSerializer.Serialize(new[] { "Python", "PyTorch", "NLP" }) }
        };
        db.Jobs.AddRange(jobs);

        // ═══════════════ GROUPS ═══════════════
        var groups = new List<Group>
        {
            new() { Name = "مجتمع مطوري الويب المصري", Description = "للمطورين المصريين — أسئلة، وظائف، كورسات، ونقاشات تقنية. كل ما يخص تطوير الويب.", CoverColor = "pattern-1", Icon = "💻", Privacy = "public", CreatorId = 2, MemberCount = 4, PostCount = 3 },
            new() { Name = "AI & Machine Learning Egypt", Description = "مجتمع متخصص في الذكاء الاصطناعي والتعلم الآلي. أبحاث، مشاريع، ونقاشات متقدمة.", CoverColor = "pattern-2", Icon = "🤖", Privacy = "public", CreatorId = 3, MemberCount = 4, PostCount = 2 },
            new() { Name = "Fresh Graduates Hub", Description = "منصة للخريجين الجدد — فرص تدريب، نصائح CV، ومقابلات. مجتمع داعم لبداية الرحلة المهنية.", CoverColor = "pattern-3", Icon = "🎓", Privacy = "public", CreatorId = 1, MemberCount = 5, PostCount = 2 },
            new() { Name = "UI/UX Designers Egypt", Description = "مجتمع المصممين المصريين — مشاركة أعمال، نقد بناء، فرص عمل، وأحدث الأدوات.", CoverColor = "pattern-4", Icon = "🎨", Privacy = "public", CreatorId = 5, MemberCount = 2, PostCount = 1 },
            new() { Name = "Freelancers بالعربي", Description = "كل ما يخص العمل الحر — عملاء، نصائح، أسعار، تجارب، ومنصات.", CoverColor = "pattern-1", Icon = "💼", Privacy = "public", CreatorId = 6, MemberCount = 3, PostCount = 1 },
            new() { Name = "Problem Solving & CP", Description = "مسائل، مسابقات، ونقاشات خوارزميات — من Beginner لـ Advanced.", CoverColor = "pattern-2", Icon = "🧩", Privacy = "public", CreatorId = 1, MemberCount = 3, PostCount = 1 }
        };
        db.Groups.AddRange(groups);
        await db.SaveChangesAsync();

        // ═══════════════ GROUP MEMBERS ═══════════════
        var memberships = new List<GroupMember>
        {
            new() { GroupId = 1, UserId = 2, Role = "admin" },
            new() { GroupId = 1, UserId = 1, Role = "member" },
            new() { GroupId = 1, UserId = 6, Role = "member" },
            new() { GroupId = 1, UserId = 8, Role = "member" },
            new() { GroupId = 2, UserId = 3, Role = "admin" },
            new() { GroupId = 2, UserId = 1, Role = "member" },
            new() { GroupId = 2, UserId = 7, Role = "member" },
            new() { GroupId = 2, UserId = 4, Role = "member" },
            new() { GroupId = 3, UserId = 1, Role = "admin" },
            new() { GroupId = 3, UserId = 3, Role = "member" },
            new() { GroupId = 3, UserId = 6, Role = "member" },
            new() { GroupId = 3, UserId = 8, Role = "member" },
            new() { GroupId = 3, UserId = 5, Role = "member" },
            new() { GroupId = 4, UserId = 5, Role = "admin" },
            new() { GroupId = 4, UserId = 1, Role = "member" },
            new() { GroupId = 5, UserId = 6, Role = "admin" },
            new() { GroupId = 5, UserId = 1, Role = "member" },
            new() { GroupId = 5, UserId = 4, Role = "member" },
            new() { GroupId = 6, UserId = 1, Role = "admin" },
            new() { GroupId = 6, UserId = 2, Role = "member" },
            new() { GroupId = 6, UserId = 8, Role = "member" }
        };
        db.GroupMembers.AddRange(memberships);

        // ═══════════════ CONNECTIONS ═══════════════
        var connections = new List<Connection>
        {
            new() { RequesterId = 1, RecipientId = 2, Status = "accepted", Circle = "professional", AcceptedAt = DateTime.UtcNow.AddDays(-30) },
            new() { RequesterId = 1, RecipientId = 3, Status = "accepted", Circle = "close", AcceptedAt = DateTime.UtcNow.AddDays(-25) },
            new() { RequesterId = 1, RecipientId = 4, Status = "accepted", Circle = "learning", AcceptedAt = DateTime.UtcNow.AddDays(-20) },
            new() { RequesterId = 1, RecipientId = 5, Status = "accepted", Circle = "professional", AcceptedAt = DateTime.UtcNow.AddDays(-15) },
            new() { RequesterId = 1, RecipientId = 6, Status = "accepted", Circle = "close", AcceptedAt = DateTime.UtcNow.AddDays(-10) },
            new() { RequesterId = 2, RecipientId = 3, Status = "accepted", Circle = "professional", AcceptedAt = DateTime.UtcNow.AddDays(-40) },
            new() { RequesterId = 2, RecipientId = 4, Status = "accepted", Circle = "professional", AcceptedAt = DateTime.UtcNow.AddDays(-35) },
            new() { RequesterId = 3, RecipientId = 5, Status = "accepted", Circle = "learning", AcceptedAt = DateTime.UtcNow.AddDays(-28) },
            new() { RequesterId = 4, RecipientId = 6, Status = "accepted", Circle = "professional", AcceptedAt = DateTime.UtcNow.AddDays(-22) },
            new() { RequesterId = 1, RecipientId = 8, Status = "accepted", Circle = "close", AcceptedAt = DateTime.UtcNow.AddDays(-5) },
            new() { RequesterId = 7, RecipientId = 1, Status = "pending", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        };
        db.Connections.AddRange(connections);

        // ═══════════════ POSTS ═══════════════
        var posts = new List<Post>
        {
            // Feed posts
            new() { UserId = 2, Body = "مبسوط إني أعلن عن أول كورس ليا على منصة Linkdout! 🎉 الكورس عن React 19 والمميزات الجديدة — مجاني بالكامل. المحتوى معمول بعناية وبالعربي عشان يوصل لكل مبتدئ. لو مهتم، سجلوا في الرابط تحت 👇", Tags = JsonSerializer.Serialize(new[] { "React", "JavaScript", "Web Dev" }), LikeCount = 42, CommentCount = 12 },
            new() { UserId = 3, Body = "بعد 6 شهور من البحث، قدرت ألاقي internship في مجال الـ Machine Learning. الفرق بين أول ما بدأت ودلوقتي كبير — 3 مشاريع عملية و portfolio قوي.\n\nنصيحتي: متستناش الشغل يجيلك، ابنيه بإيدك 🚀", Tags = JsonSerializer.Serialize(new[] { "Machine Learning", "Career", "Internship" }), LikeCount = 87, CommentCount = 24 },
            new() { UserId = 4, Body = "بنفتح باب التقديم لبرنامج التدريب الصيفي في TechVillage! 🔥\n\nالمجالات:\n• Frontend (React/Next.js)\n• Backend (Node.js/Python)\n• AI & Machine Learning\n\nالبرنامج 3 شهور، مدفوع، وفرصة للتعيين بعد كده. كل اللي محتاجه: مشروع شخصي واحد على الأقل + أساسيات قوية.\n\nDeadline: 20 يونيو. ابعتوا الـ CV على الرابط في أول كومنت 👇", Tags = JsonSerializer.Serialize(new[] { "Internship", "AI", "Web Dev", "فرصة" }), LikeCount = 123, CommentCount = 47 },
            new() { UserId = 5, Body = "أهم 5 نصائح لكل مصمم UI/UX مبتدئ:\n\n1. ابدأ بنسخ تطبيقات موجودة — دي أسرع طريقة تتعلم\n2. اقرأ الـ Design System بتاع شركات كبيرة (Material, Human Interface)\n3. شارك شغلك على Behance واطلب feedback\n4. تعلم الأساسيات قبل الـ tools — Figma أداة مش هدف\n5. ابنِ case studies مش مجرد شاشات\n\nحفظتوا؟ 👀", Tags = JsonSerializer.Serialize(new[] { "UI/UX", "Design", "Tips" }), LikeCount = 98, CommentCount = 21 },
            new() { UserId = 1, Body = "أول منشور ليا على Linkdout! 🦊\n\nمتحمس جداً أبدأ الرحلة دي معاكم. شغال حالياً على:\n• تعلم Machine Learning (المرحلة الأولى — الأساسيات)\n• بناء منصة Linkdout نفسها (Full Stack)\n• تجهيز محتوى جديد لقناتي على اليوتيوب\n\nلو حد في نفس المجال وعايز نتعلم مع بعض — أنا موجود! 💪", Tags = JsonSerializer.Serialize(new[] { "AI", "Learning", "Web Dev" }), LikeCount = 56, CommentCount = 15 },

            // Group posts
            new() { UserId = 2, GroupId = 1, Body = "حد جرب Next.js 15 مع React Server Components؟ عايز أعرف التجربة العملية — هل فعلاً الأداء فرق ولا لسه بدري؟ 🤔", Tags = JsonSerializer.Serialize(new[] { "Next.js", "React" }), LikeCount = 23, CommentCount = 8 },
            new() { UserId = 6, GroupId = 1, Body = "أول Freelance project خلصته على Upwork! 🎉 كان موقع بسيط بــ React + Node.js. الحمد لله العميل مبسوط والتقييم 5 نجوم. اللي لسه بادئ — متيأسش، أول عميل هو الأصعب وبعد كده الدنيا بتمشي.", Tags = JsonSerializer.Serialize(new[] { "Freelance", "Upwork", "React" }), LikeCount = 67, CommentCount = 18 },
            new() { UserId = 8, GroupId = 1, Body = "سؤال للناس اللي شغالة بـ TypeScript: إيه أكتر حاجة فرقت معاكم في الإنتاجية بعد ما حولتوا من JavaScript؟", Tags = JsonSerializer.Serialize(new[] { "TypeScript", "JavaScript" }), LikeCount = 34, CommentCount = 22 },

            new() { UserId = 3, GroupId = 2, Body = "منشور مهم لكل مبتدئ في الـ AI:\n\nمش محتاج تبدأ بـ Deep Learning على طول. ابدأ بالأساسيات:\n1. Linear Algebra & Statistics\n2. Python + NumPy + Pandas\n3. Scikit-learn لمشاريع ML كلاسيكية\n4. بعد كده انتقل لـ Deep Learning\n\nالطريق طويل — بس ممتع 🔥", Tags = JsonSerializer.Serialize(new[] { "AI", "Roadmap", "Machine Learning" }), LikeCount = 89, CommentCount = 31 },
            new() { UserId = 7, GroupId = 2, Body = "صباح الخير! حابة أشارك ورقة بحثية نزلتها عن Reinforcement Learning للتحكم في الروبوتات. مستعدة أجاوب على أي أسئلة 👩‍🔬", Tags = JsonSerializer.Serialize(new[] { "Research", "RL", "Robotics" }), LikeCount = 45, CommentCount = 12 },

            new() { UserId = 1, GroupId = 3, Body = "سؤال للخريجين الجدد: إيه أكتر حاجة محتاجينها ومش لاقينها في مواقع التوظيف الحالية؟ بنبني منصة جديدة وعايزين نفهم احتياجاتكم بشكل حقيقي 🙏", Tags = JsonSerializer.Serialize(new[] { "Fresh Grad", "Feedback", "Opportunities" }), LikeCount = 72, CommentCount = 38 },
            new() { UserId = 3, GroupId = 3, Body = "بفتح باب الـ mentorship لـ 5 طلاب في مجال الـ Data Science. اللي مهتم يبعتلي رسالة مع الـ CV بتاعه وهختار بناءً على الشغف والمشاريع — مش الدرجات 💯", Tags = JsonSerializer.Serialize(new[] { "Mentorship", "Data Science" }), LikeCount = 156, CommentCount = 64 },

            new() { UserId = 5, GroupId = 4, Body = "شاركة سريعة: أحسن 3 مكتبات UI عربية للـ React:\n\n1. Ant Design — شامل ومتكامل (بيدعم RTL)\n2. Mantine — خفيف وسريع\n3. shadcn/ui — تحكم كامل وتخصيص\n\nإنتوا بتستخدموا إيه؟ 👇", Tags = JsonSerializer.Serialize(new[] { "React", "UI", "Libraries" }), LikeCount = 41, CommentCount = 17 },

            new() { UserId = 6, GroupId = 5, Body = "بعد سنتين في الفريلانس، دي أهم 3 حاجات اتعلمتها:\n\n1. سعرك مش مهارتك — سعرك هو قيمتك التسويقية\n2. أول 3 عملاء هما الأصعب — بعد كده الـ referrals بتشتغل لوحدها\n3. التواصل أهم من الكود — client management هو 60% من الشغل\n\nشاركونا تجاربكم 👇", Tags = JsonSerializer.Serialize(new[] { "Freelance", "Tips", "Career" }), LikeCount = 112, CommentCount = 43 },

            new() { UserId = 1, GroupId = 6, Body = "حد عنده tips لكودفورسز الجديدة (ICPC)؟ عايز أحسن استراتيجية للتدريب — هل أركز على عدد مسائل كبير ولا أعمق في مواضيع معينة؟", Tags = JsonSerializer.Serialize(new[] { "ICPC", "Codeforces", "CP" }), LikeCount = 28, CommentCount = 14 }
        };
        db.Posts.AddRange(posts);
        await db.SaveChangesAsync();
    }
}
