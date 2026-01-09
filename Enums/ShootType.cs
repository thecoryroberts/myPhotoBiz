using System.ComponentModel.DataAnnotations;

namespace MyPhotoBiz.Enums
{
    public enum ShootType
    {
        [Display(Name = "Portrait")]
        Portrait = 0,

        [Display(Name = "Wedding")]
        Wedding = 1,

        [Display(Name = "Engagement")]
        Engagement = 2,

        [Display(Name = "Family")]
        Family = 3,

        [Display(Name = "Newborn")]
        Newborn = 4,

        [Display(Name = "Maternity")]
        Maternity = 5,

        [Display(Name = "Event")]
        Event = 6,

        [Display(Name = "Corporate")]
        Corporate = 7,

        [Display(Name = "Product")]
        Product = 8,

        [Display(Name = "Real Estate")]
        RealEstate = 9,

        [Display(Name = "Headshot")]
        Headshot = 10,

        [Display(Name = "Senior")]
        Senior = 11,

        [Display(Name = "Boudoir")]
        Boudoir = 12,

        [Display(Name = "Pet")]
        Pet = 13,

        [Display(Name = "Other")]
        Other = 99
    }
}
