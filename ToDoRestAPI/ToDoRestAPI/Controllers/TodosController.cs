using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoRestAPI.Models;

namespace ToDoRestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ToDoAppContext context;

        public TodosController(ToDoAppContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
        {
            try
            {
                return Ok(await this.context.Todos.ToListAsync());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            try
            {
                var result = await this.context.Todos.FindAsync(id);
                if (result == null)
                {
                    return NotFound();
                }

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Todo>> PostTodo([FromBody] Todo todo)
        {
            try
            {
                if (todo == null)
                {
                    return BadRequest();
                }

                this.context.Todos.Add(todo);

                await this.context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTodo),
                    new { id = todo.Id }, todo);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new todo record");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Todo>> PutTodo(int id, [FromBody] Todo todo)
        {
            try
            {
                if (id  != todo.Id)
                {
                    return BadRequest("Todo id mismatch");
                }

                this.context.Entry(todo).State = EntityState.Modified;

                try
                {
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoExists(id))
                    {
                        return NotFound($"Todo with Id = {id} not found");
                    }
                    else
                    {
                        throw;
                    }
                }

                return CreatedAtAction(nameof(GetTodo),
                    new { id = todo.Id }, todo);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error modify todo record");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Todo>> DeleteTodo(int id)
        {
            try
            {
                var todo = await this.context.Todos.FindAsync(id);
                if (todo == null)
                {
                    return NotFound();
                }

                //this.context.Todos.Remove(todo);
                this.context.Entry(todo).State = EntityState.Deleted;
                await this.context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error delete todo record");
            }
        }

        private bool TodoExists(int id)
        {
            return this.context.Todos.Any(x => x.Id == id);
        }
    }
}
