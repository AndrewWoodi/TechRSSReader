﻿using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechRSSReader.Application.Blogs.Queries.GetBlogs;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Domain.Entities;
using TechRSSReader.Domain.ValueObjects;

namespace TechRSSReader.Application.Blogs.Commands.CreateBlog
{
    public class CreateBlogCommand: IRequest<BlogDto>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string XmlAddress { get; set; }

        public List<KeywordToExcludeDto> KeywordsToExclude { get; set; }

        public List<KeywordToIncludeDto> KeywordsToInclude { get; set; }

        public class CreateBlogCommandHandler : IRequestHandler<CreateBlogCommand, BlogDto>
        {

            private readonly IApplicationDbContext _context;
            private readonly IMapper _mapper;
            private readonly ICurrentUserService _currentUserService;

            public CreateBlogCommandHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
            {
                _context = context;
                _mapper = mapper;
                _currentUserService = currentUserService;
            }

            public async Task<BlogDto> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
            {
                Blog blog = new Blog
                {
                    Title = request.Title,
                    XmlAddress = request.XmlAddress
                };

                foreach (KeywordToExcludeDto keywordToExclude in request.KeywordsToExclude)
                {
                    blog.KeywordsToExclude.Add(new KeywordToExclude { BlogId = blog.Id, Keyword = keywordToExclude.Keyword });

                }

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync(_currentUserService.UserId, cancellationToken);
               
                return _mapper.Map<BlogDto>(blog);
            }
        }

    }
}
