using System;
using AppreciateUI.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace AppreciateUI.Controllers
{
    public class ScrapbookCategoryViewController : BaseDialogViewController
    {
        public Project Project { get; set; }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var scraps = new Dictionary<string, List<ProjectImage>>(StringComparer.InvariantCultureIgnoreCase);
            var images = Data.Database.Main.Table<ProjectImage>().Where(a => a.ProjectId == Project.Id);

            if (images.Count() == 0)
            {
                NavigationController.PopViewControllerAnimated(true);
            }

            foreach (var img in images)
            {
                var category = img.Category ?? "Uncategorized";


                if (scraps.ContainsKey(category))
                    scraps[category].Add(img);
                else
                    scraps[category] = new List<ProjectImage> { img };
            }

            var allItems = new Section("All");
            var e = new StyledStringElement("All Patterns", images.Count().ToString(), UITableViewCellStyle.Value1) 
                              { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            e.Tapped += () => NavigationController.PushViewController(new LocalViewPatternsViewController(new List<ProjectImage>(images)) { Title = "All Patterns" }, true);
            allItems.Add(e);

            var categories = new Section("Categories");
            foreach (var k in scraps.Keys)
            {
                var key = k;
                var element = new StyledStringElement(key, scraps[key].Count.ToString(), UITableViewCellStyle.Value1) 
                              { Accessory = UITableViewCellAccessory.DisclosureIndicator };
                element.Tapped += () => NavigationController.PushViewController(new LocalViewPatternsViewController(scraps[key]) { Title = key }, true);
                categories.Add(element);
            }

            var root = new RootElement(Title) {allItems, categories};
            Root = root;
        }
    }
}

