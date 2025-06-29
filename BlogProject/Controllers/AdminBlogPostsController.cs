using Microsoft.AspNetCore.Mvc;
using MyFirstApp.Models.ViewModels;
using MyFirstApp.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MyFirstApp.Models.Domain;
using MyFirstApp.Repositories;
using Microsoft.AspNetCore.Authorization;


namespace MyFirstApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBlogPostsController : Controller
    {
        private readonly ITagRepository tagRepository;
        private readonly IBlogPostRepository blogPostRepository;

        public AdminBlogPostsController(ITagRepository tagRepository, IBlogPostRepository blogPostRepository)
        {
            this.tagRepository = tagRepository;
            this.blogPostRepository = blogPostRepository;
        }

        [HttpGet]
       public async Task<IActionResult> Add()
        {
            //get tags from repository
            var tags = await tagRepository.GetAllAsync();
            var model = new AddBlogPostRequest
            {
                Tags = tags.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            };
            return View(model);
        }

        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> Add(AddBlogPostRequest addBlogPostRequest)

        {
            //Map view model to domain model
            var blogPost = new BlogPost
            {
                Heading = addBlogPostRequest.Heading,
                PageTitle = addBlogPostRequest.PageTitle,
                Content = addBlogPostRequest.Content,
                ShortDescription = addBlogPostRequest.ShortDescription,
                FeaturedImageUrl = addBlogPostRequest.FeaturedImageUrl,
                UrlHandle = addBlogPostRequest.UrlHandle,
                PublishedDate = addBlogPostRequest.PublishedDate,
                Author = addBlogPostRequest.Author,
                Visible = addBlogPostRequest.Visible,
        
            };

            //Map tags from selected tags
            var selectedTags = new List<Tag>();
            foreach(var selectedTagId in addBlogPostRequest.SelectedTags)
            {
                var selectedTagIdAsGuid = Guid.Parse(selectedTagId);
                var existingTag = await tagRepository.GetAsync(selectedTagIdAsGuid);

                if (existingTag != null) { 
                    selectedTags.Add(existingTag);
                }
            }
            //Mapping tags back to domain model
            blogPost.Tags = selectedTags;

            await blogPostRepository.AddAsync(blogPost);
            return RedirectToAction("Add");

        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            //Call the repository
            var blogPosts = await blogPostRepository.GetAllAsync();
            return View(blogPosts);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            //Retrieve the result from the Repository
            var blogPost = await blogPostRepository.GetAsync(id);
            var tagsDomainModel = await tagRepository.GetAllAsync();

            if (blogPost != null)
            {
                //map the domain model into the model
                var model = new EditBlogPostRequest
                {
                    Id = blogPost.Id,
                    Heading = blogPost.Heading,
                    PageTitle = blogPost.PageTitle,
                    Content = blogPost.Content,
                    Author = blogPost.Author,
                    FeaturedImageUrl = blogPost.FeaturedImageUrl,
                    UrlHandle = blogPost.UrlHandle,
                    ShortDescription = blogPost.ShortDescription,
                    PublishedDate = blogPost.PublishedDate,
                    Visible = blogPost.Visible,
                    Tags = tagsDomainModel.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                    SelectedTags = blogPost.Tags.Select(x => x.Id.ToString()).ToArray()

                };
                return View(model);
            }
            
            //pass data to view
            return View(null);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(EditBlogPostRequest editBlogPostRequest)
        {
            //map viewmodel back to domain model
            var blogPostDomainModel = new BlogPost
            {
                Id = editBlogPostRequest.Id,
                Heading = editBlogPostRequest.Heading,
                PageTitle = editBlogPostRequest.PageTitle,
                Content = editBlogPostRequest.Content,
                Author = editBlogPostRequest.Author,
                ShortDescription = editBlogPostRequest.ShortDescription,
                FeaturedImageUrl = editBlogPostRequest.FeaturedImageUrl,
                PublishedDate = editBlogPostRequest.PublishedDate,
                UrlHandle = editBlogPostRequest.UrlHandle,
                Visible = editBlogPostRequest.Visible
            };

            //map tags into domain model
            var selectedTags = new List<Tag>();
            foreach(var selectedTag in editBlogPostRequest.SelectedTags)
            {
                if(Guid.TryParse(selectedTag, out var tag))
                {
                    var foundTag = await tagRepository.GetAsync(tag);
                    if(foundTag != null)
                    {
                        selectedTags.Add(foundTag);
                    }
                }
            }
            blogPostDomainModel.Tags = selectedTags;

            //submit information to repository to update
            var updatedBlog = await blogPostRepository.UpdateAsync(blogPostDomainModel);

            if(updatedBlog != null)
            {
                //show success notification
                return RedirectToAction("Edit");
            }
            //show error notification
            return RedirectToAction("Edit");

        }

        [HttpPost]
        public async Task<IActionResult> Delete(EditBlogPostRequest editBlogPostRequest)
        {
            //Talk to repository to delete this blogpost and tag
            var deletedBlogPost = await blogPostRepository.DeleteAsync(editBlogPostRequest.Id);

            if(deletedBlogPost != null)
            {
                //show success notification
                return RedirectToAction("List");
            }

            //show error notification
            return RedirectToAction("Edit", new {id = editBlogPostRequest.Id});
        }
    }
}
