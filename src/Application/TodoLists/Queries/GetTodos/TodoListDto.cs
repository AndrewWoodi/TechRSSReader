﻿using TechRSSReader.Application.Common.Mappings;
using TechRSSReader.Domain.Entities;
using System.Collections.Generic;

namespace TechRSSReader.Application.TodoLists.Queries.GetTodos
{
    public class TodoListDto : IMapFrom<TodoList>
{
    public TodoListDto()
    {
        Items = new List<TodoItemDto>();
    }

    public int Id { get; set; }

    public string Title { get; set; }

    public IList<TodoItemDto> Items { get; set; }
}
}
