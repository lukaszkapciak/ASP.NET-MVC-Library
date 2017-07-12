using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Library.Context;
using Library.Models;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using Library.ModelView;

namespace Library.Controllers
{
    public class RentedBooksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string search = null)
        {
            var model = db.RentedBooks.ToList();
            List <RentedBooksAdminModelView> rentedBooks = new List<RentedBooksAdminModelView>();
            foreach (var item in model)
            {
                Book book = db.Books.Find(item.BookId);
                RentedBooksAdminModelView rentedBook = new RentedBooksAdminModelView();
                rentedBook.RentedBookId = item.RentedBookId;
                rentedBook.BookId = item.BookId;
                rentedBook.Title = book.Title;
                rentedBook.UserName = item.UserName;
                rentedBook.RentDate = item.RentDate;
                rentedBook.ReturnDate = item.ReturnDate;
                rentedBook.Penalty = item.Penalty;
                if (DateTime.Now > rentedBook.ReturnDate)
                    rentedBook.Penalty = rentedBook.Penalty + 1 * DateTime.Now.Second - rentedBook.ReturnDate.Second;
                rentedBooks.Add(rentedBook);
            }
                return View(rentedBooks);
        }

        public ActionResult RentConfirmation(int id)
        {
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            var model = new RentedBook();
            model.BookId = id;
            model.RentDate = DateTime.Now;
            model.ReturnDate = model.RentDate.AddDays(14);
            model.Penalty = 0;
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RentConfirmation([Bind(Include = "RentedBookId,BookId,UserId,RentDate,ReturnDate,Penalty")] RentedBook rentedBook)
        {
            rentedBook.UserName = User.Identity.GetUserName();
            rentedBook.RentDate = DateTime.Now;
            rentedBook.ReturnDate = rentedBook.RentDate.AddDays(14);

            Book book = db.Books.Find(rentedBook.BookId);
            book.Available = false;
            db.Entry(book).State = EntityState.Modified;

            if (ModelState.IsValid)
            {
                db.RentedBooks.Add(rentedBook);
                db.SaveChanges();
                return RedirectToAction("Index", "Books");
            }

            return View(rentedBook);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RentedBook rentedBook = db.RentedBooks.Find(id);
            if (rentedBook == null)
            {
                return HttpNotFound();
            }
            return View(rentedBook);
        }

        // POST: RentedBooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RentedBook rentedBook = db.RentedBooks.Find(id);
            Book book = db.Books.Find(rentedBook.BookId);
            book.Available = true;

            db.RentedBooks.Remove(rentedBook);
            db.Entry(book).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
