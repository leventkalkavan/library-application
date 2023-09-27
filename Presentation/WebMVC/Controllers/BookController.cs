using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Repositories.BookRepositories;
using Application.Repositories.CustomerRepositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMVC.Models.ViewModels;

namespace WebMVC.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookReadRepository _bookReadRepository;
        private readonly IBookWriteRepository _bookWriteRepository;
        private readonly IOrderWriteRepository _orderWriteRepository;
        public BookController(IBookReadRepository bookReadRepository, IBookWriteRepository bookWriteRepository, IOrderWriteRepository orderWriteRepository)
        {
            _bookReadRepository = bookReadRepository;
            _bookWriteRepository = bookWriteRepository;
            _orderWriteRepository = orderWriteRepository;
        }
        
        [HttpGet]
        public IActionResult ListBook()
        {
            var books = _bookReadRepository.GetAll().Include(b => b.Order).ToList();
            return View(books);
        }

        
        [HttpGet]
        public IActionResult AddBook()
        { 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); 
            }

            var book = new Book
            {
                Name = model.BookName,
                AuthorName = model.AuthorName,
                BookPhotoUrl = model.BookPhotoUrl,
                Status = true
            };
            await _bookWriteRepository.AddAsync(book);
            await _bookWriteRepository.SaveAsync();

            return RedirectToAction("ListBook");
        }


        [HttpGet]
        public IActionResult HiringBook()
        {
            var books = _bookReadRepository.GetAll().Where(x=>x.Status == true);
            ViewBag.Books = books;

            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> HiringBook(HiringBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            
            var book = await _bookReadRepository.GetByIdAsync(model.BookId);

            book.Status = false; 
            _bookWriteRepository.Update(book);
            await _bookWriteRepository.SaveAsync();

            var order = new Order
            {
                Name = model.RentName,
                ReturnDateTime = model.ReturnDateTime.Date,
                BookId = model.BookId,
            };

            await _orderWriteRepository.AddAsync(order);
            await _orderWriteRepository.SaveAsync();
            
            return RedirectToAction("ListBook");
        }

    }
}