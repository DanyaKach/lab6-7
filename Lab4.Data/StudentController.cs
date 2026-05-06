using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Lab4.Data;
[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly StudentRepository _repository;
    private readonly IValidator<CreateStudentRequest> _createValidator;
    private readonly IValidator<UpdateStudentRequest> _updateValidator;
    
    public StudentController(
        StudentRepository repository,
        IValidator<CreateStudentRequest> createValidator,
        IValidator<UpdateStudentRequest> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateStudentRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Errors = errors });
        }
        var student = new Student
        {
            FullName = request.FullName,
            Email = request.Email,
            EnrollmentDate = request.EnrollmentDate
        };
        await _repository.AddAsync(student);
        return Created("", student);
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateStudentRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { Errors = errors });
        }
        var student = await _repository.GetByIdAsync(request.Id);
        if (student != null)
        {
            student.FullName = request.FullName;
            student.Email = request.Email;
            await _repository.UpdateAsync(student);
            return Ok(student);
        }
        return NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var students = await _repository.GetAllAsync();
        return Ok(students);
    }
}
