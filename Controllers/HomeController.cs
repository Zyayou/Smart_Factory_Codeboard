using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFactory_MinProject.Models;
using System.Diagnostics;

namespace SmartFactory_MinProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CodeboardDbContext _context;

        public HomeController(ILogger<HomeController> logger, CodeboardDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Login()
        {

            //넘어온 세션의 값이 null일 경우 Login페이지로 바로 가기
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                //세션의 내용을 ViewBag에 담습니다.
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }

            return View();
        }
        [HttpPost]
        public IActionResult Login(User user)
        {
            //SqlServer에 있는 Id/password와 폼에서 입력한 id/password를 비교합니다.
            var myUser = _context.User.Where(
                x => x.AccountId == user.AccountId &&
                     x.AccountPassword == user.AccountPassword)
                .FirstOrDefault();

            if (myUser != null)
            {
                //세션을 만드는 코드 입니다.
                //세션의 정보를 [Key, Value] 조합으로 만듭니다. Key는 UserSession이란 단어 Value는 DB에 있는 email 값입니다.
                HttpContext.Session.SetString("UserSession", myUser.AccountId);
                HttpContext.Session.SetString("UserName", myUser.Name);

                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Message = "아이디 혹은 비밀번호가 일치하지 않습니다.";
            }

            return View();
        }


        public IActionResult Logout()
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                //세션정보를 삭제합니다.
                HttpContext.Session.Remove("UserSession");
                HttpContext.Session.Remove("UserName");
                //return RedirectToAction("Login", "Home");  //로그아웃 후 바로 로그인 화면으로 이동
            }
            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            //SqlServer에 있는 Id/password와 폼에서 입력한 id/password를 비교합니다.
            var myUser = _context.User.Where(
                x => x.AccountId == user.AccountId)
                .FirstOrDefault();

            if (myUser == null)
            {
                if (ModelState.IsValid)
                {
                    await _context.User.AddAsync(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Login", "Home");
                }
                return View(user);
            }
            else
            {
                ViewBag.Message = "중복된 아이디가 존재합니다.";

                return View();
            }
        }



        public async Task<IActionResult> Index(string searchTerm)
        {
            if (HttpContext.Session.GetString("UserSession") == null)
            {
                return RedirectToAction("Login");
            }

            var codeboard = await _context.Codeboard.ToListAsync(); // 기본 목록

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // 검색어로 필터링
                codeboard = codeboard.Where(c => c.Title.Contains(searchTerm) || c.Name.Contains(searchTerm)).ToList();
            }

            ViewBag.MySession = HttpContext.Session.GetString("UserName").ToString();
            return View(codeboard);
        }




        public IActionResult Create()
        {
            // 넘어온 세션의 값이 null이 아닐 경우 즉, 로그인 작업으로 세션이 만들어져 있는 경우
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                //세션의 내용을 ViewBag에 담습니다.
                ViewBag.MySession = HttpContext.Session.GetString("UserName").ToString();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Codeboard codeboard)
        {
            // 넘어온 세션의 값이 null이 아닐 경우 즉, 로그인 작업으로 세션이 만들어져 있는 경우
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                //세션의 내용을 ViewBag에 담습니다.
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }

            if (ModelState.IsValid)
            {
                await _context.Codeboard.AddAsync(codeboard);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(codeboard);

        }

        [HttpPost]
        public IActionResult DeleteComment(int commentId)
        {
            var comment = _context.Comments.Find(commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }

            // 이전 페이지로 리디렉션 또는 다른 동작을 수행할 수 있습니다.
            return RedirectToAction("Details", new { id = comment.CodeboardId });
        }








        public IActionResult Details(int? id)
        {
            if (id == null || _context.Codeboard == null)
            {
                return NotFound();
            }
            var codeboard = _context.Codeboard
                .Include(c => c.Comments) // 댓글들을 가져오기 위해 Include 사용
                .FirstOrDefault(x => x.Id == id);

            if (codeboard == null)
            {
                return NotFound();
            }

            return View(codeboard);
        }



        [HttpPost]
        public IActionResult AddComment(int id, string content)
        {
            var codeboard = _context.Codeboard.Find(id);

            if (content != null)
            {

                if (codeboard != null)
                {
                    var comment = new Comment
                    {
                        Content = content,
                        Date = DateTime.Now,
                        CodeboardId = id
                    };

                    _context.Comments.Add(comment);
                    _context.SaveChanges();
                }

            }
            return RedirectToAction("Details", new { id });

        }




        public IActionResult Edit(int? id)
        {
            if (id == null || _context.Codeboard == null)
            {
                return NotFound();
            }
            var codeboard = _context.Codeboard.Find(id);

            if (codeboard == null)
            {
                return NotFound();
            }
            return View(codeboard);
        }
        [HttpPost]
        public IActionResult Edit(int? id, Codeboard std)
        {
            if (ModelState.IsValid)
            {
                _context.Update(std);
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(std);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || _context.Codeboard == null)
            {
                return NotFound();
            }
            var stdData = _context.Codeboard.FirstOrDefault(x => x.Id == id);

            if (stdData == null)
            {
                return NotFound();
            }
            return View(stdData);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int? id)
        {
            var stdData = _context.Codeboard.Find(id);

            if (stdData != null)
            {
                _context.Codeboard.Remove(stdData);
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}