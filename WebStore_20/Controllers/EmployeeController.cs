﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStore.Interfaces.Infrastructure;
using WebStore.ViewModels;

namespace WebStore.Controllers
{
    //[Route("users/[action]")]
    [Route("users")]
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeesService _employeesService;

        public EmployeeController(IEmployeesService employeesService)
        {
            _employeesService = employeesService;
        }

        [Route("all")]
        [AllowAnonymous]
        public IActionResult Employees()
        {
            return View(_employeesService.GetAll());
        }

        [Route("{id}")]
        [Authorize(Roles = "Admins, Users")] 
        public IActionResult EmployeeDetails(int id)
        {
            //Получаем сотрудника по Id
            var employee = _employeesService.GetById(id);
            
            //Если такого не существует
            if (employee == null)
                return NotFound(); // возвращаем результат 404 Not Found
 
            //Иначе возвращаем сотрудника
            return View(employee);
        }

        /// <summary>
        /// Добавление или редактирование сотрудника
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("edit/{id?}")]
        [Authorize(Roles = "Admins")]
        public IActionResult Edit(int? id)
        {
            if (!id.HasValue)
                return View(new EmployeeViewModel());

            var model = _employeesService.GetById(id.Value);
            if (model == null)
                return NotFound();// возвращаем результат 404 Not Found

            return View(model);
        }

        [HttpPost]
        [Route("edit/{id?}")]
        [Authorize(Roles = "Admins")]
        public IActionResult Edit(EmployeeViewModel model)
        {
            if (model.Age < 18 || model.Age > 100)
            {
                ModelState.AddModelError("Age", "Ошибка возраста!");
            }

            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                // Если не валидна, возвращаем ее на представление
                return View(model);
            }

            if (model.Id > 0) // если есть Id, то редактируем модель
            {
                var dbItem = _employeesService.GetById(model.Id);

                if (ReferenceEquals(dbItem, null))
                    return NotFound();// возвращаем результат 404 Not Found

                dbItem.FirstName = model.FirstName;
                dbItem.SurName = model.SurName;
                dbItem.Age = model.Age;
                dbItem.Patronymic = model.Patronymic;
                dbItem.Position = model.Position;
            }
            else // иначе добавляем модель в список
            {
                _employeesService.AddNew(model);
            }
            _employeesService.Commit(); // станет актуальным позднее (когда добавим БД)

            return RedirectToAction(nameof(Employees));
        }

        /// <summary>
        /// Удаление сотрудника
        /// </summary>
        /// <param name="id">Id сотрудника</param>
        /// <returns></returns>
        [Route("delete/{id}")]
        [Authorize(Roles = "Admins")]
        public IActionResult Delete(int id)
        {
            _employeesService.Delete(id);
            return RedirectToAction(nameof(Employees));
        }

    }
}
