// Initialize Lucide icons and add modern interactions
document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();

  // Add intersection observer for scroll animations
  const observerOptions = {
    threshold: 0.1,
    rootMargin: "0px 0px -50px 0px",
  };

  const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add("animate-fade-in-up");
      }
    });
  }, observerOptions);

  // Observe all cards for scroll animations
  document.querySelectorAll(".group").forEach((card) => {
    observer.observe(card);
  });

  // Add smooth hover effects for cards
  document.querySelectorAll(".group").forEach((card) => {
    card.addEventListener("mouseenter", function () {
      this.style.transform = "translateY(-5px) scale(1.02)";
    });

    card.addEventListener("mouseleave", function () {
      this.style.transform = "translateY(0) scale(1)";
    });
  });

  // Add click ripple effect
  document.querySelectorAll('[onclick*="playTrack"]').forEach((element) => {
    element.addEventListener("click", function (e) {
      const ripple = document.createElement("div");
      const rect = this.getBoundingClientRect();
      const size = Math.max(rect.width, rect.height);
      const x = e.clientX - rect.left - size / 2;
      const y = e.clientY - rect.top - size / 2;

      ripple.style.cssText = `
                    position: absolute;
                    width: ${size}px;
                    height: ${size}px;
                    left: ${x}px;
                    top: ${y}px;
                    background: rgba(255, 85, 0, 0.3);
                    border-radius: 50%;
                    transform: scale(0);
                    animation: ripple 0.6s ease-out;
                    pointer-events: none;
                `;

      this.style.position = "relative";
      this.style.overflow = "hidden";
      this.appendChild(ripple);

      setTimeout(() => ripple.remove(), 600);
    });
  });

  // Parallax effect for background elements
  window.addEventListener("scroll", () => {
    const scrolled = window.pageYOffset;
    const parallaxElements = document.querySelectorAll(".absolute");

    parallaxElements.forEach((element) => {
      const speed = 0.5;
      const yPos = -(scrolled * speed);
      element.style.transform = `translateY(${yPos}px)`;
    });
  });
}); // Add CSS animation keyframes
const style = document.createElement("style");
style.textContent =
  "@@keyframes ripple {" +
  "to {" +
  "transform: scale(4);" +
  "opacity: 0;" +
  "}" +
  "}" +
  "@@keyframes float {" +
  "0%, 100% { transform: translateY(0px); }" +
  "50% { transform: translateY(-10px); }" +
  "}" +
  ".float-animation {" +
  "animation: float 3s ease-in-out infinite;" +
  "}";
document.head.appendChild(style);
