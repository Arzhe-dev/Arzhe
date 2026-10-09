// ============================================================
//  ARZHE - Le site ULTIME
// ============================================================

// ============ SPLASH ============
window.addEventListener('load', () => {
  const splash = document.getElementById('splash');
  setTimeout(() => {
    if (splash) splash.classList.add('hidden');
  }, 1500);
});

// ============ CURSEUR CUSTOM ============
const cursor = document.getElementById('cursor');
const cursorDot = document.getElementById('cursorDot');
let mouseX = 0, mouseY = 0, cursorX = 0, cursorY = 0;

document.addEventListener('mousemove', (e) => {
  mouseX = e.clientX;
  mouseY = e.clientY;
  if (cursorDot) {
    cursorDot.style.transform = 'translate(' + (mouseX - 2.5) + 'px, ' + (mouseY - 2.5) + 'px)';
  }
}, { passive: true });

function animateCursor() {
  cursorX += (mouseX - cursorX) * 0.18;
  cursorY += (mouseY - cursorY) * 0.18;
  if (cursor) {
    cursor.style.transform = 'translate(' + (cursorX - 16) + 'px, ' + (cursorY - 16) + 'px)';
  }
  requestAnimationFrame(animateCursor);
}
animateCursor();

// Hover state
document.querySelectorAll('a, button, .feature-card, .showcase-mockup, [data-tilt]').forEach(el => {
  el.addEventListener('mouseenter', () => cursor && cursor.classList.add('hover'));
  el.addEventListener('mouseleave', () => cursor && cursor.classList.remove('hover'));
});

// ============ SCROLL PROGRESS + NAV ============
const scrollProgress = document.getElementById('scrollProgress');
const nav = document.getElementById('nav');
let lastScrollY = 0;
let ticking = false;

window.addEventListener('scroll', () => {
  lastScrollY = window.scrollY;
  if (!ticking) {
    requestAnimationFrame(updateScroll);
    ticking = true;
  }
}, { passive: true });

function updateScroll() {
  const scrollTop = lastScrollY;
  const docHeight = document.documentElement.scrollHeight - window.innerHeight;
  const scrollPercent = docHeight > 0 ? (scrollTop / docHeight) * 100 : 0;

  if (scrollProgress) scrollProgress.style.width = scrollPercent + '%';
  if (nav) {
    if (scrollTop > 50) nav.classList.add('scrolled');
    else nav.classList.remove('scrolled');
  }
  ticking = false;
}

// ============ REVEAL ============
const revealObserver = new IntersectionObserver((entries) => {
  entries.forEach((entry, index) => {
    if (entry.isIntersecting) {
      setTimeout(() => {
        entry.target.classList.add('visible');
      }, index * 80);
      revealObserver.unobserve(entry.target);
    }
  });
}, { threshold: 0.1, rootMargin: '0px 0px -80px 0px' });

document.querySelectorAll('.reveal').forEach(el => revealObserver.observe(el));

// ============ MAGNETIC ============
document.querySelectorAll('.magnetic').forEach(btn => {
  btn.addEventListener('mousemove', (e) => {
    const rect = btn.getBoundingClientRect();
    const x = e.clientX - rect.left - rect.width / 2;
    const y = e.clientY - rect.top - rect.height / 2;
    btn.style.transform = 'translate(' + (x * 0.3) + 'px, ' + (y * 0.3) + 'px)';
  });
  btn.addEventListener('mouseleave', () => {
    btn.style.transform = '';
  });
});

// ============ TILT 3D ============
document.querySelectorAll('[data-tilt]').forEach(card => {
  card.addEventListener('mousemove', (e) => {
    const rect = card.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const centerX = rect.width / 2;
    const centerY = rect.height / 2;
    const rotateX = ((y - centerY) / centerY) * -5;
    const rotateY = ((x - centerX) / centerX) * 5;

    card.style.transform = 'perspective(1000px) rotateX(' + rotateX + 'deg) rotateY(' + rotateY + 'deg) translateY(-8px)';

    // Position pour le glow interne
    const mx = (x / rect.width) * 100;
    const my = (y / rect.height) * 100;
    card.style.setProperty('--mx', mx + '%');
    card.style.setProperty('--my', my + '%');
  });
  card.addEventListener('mouseleave', () => {
    card.style.transform = '';
  });
});

// ============ HERO PARALLAX ============
const heroGrid = document.querySelector('.hero-grid');
window.addEventListener('scroll', () => {
  if (heroGrid) {
    heroGrid.style.transform = 'translateY(' + (window.scrollY * 0.3) + 'px)';
  }
}, { passive: true });

// ============ COMPTEURS ANIMES ============
const counters = document.querySelectorAll('[data-count]');
const counterObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      const el = entry.target;
      const target = parseInt(el.getAttribute('data-count'));
      const duration = 1800;
      const start = performance.now();

      function update(now) {
        const elapsed = now - start;
        const progress = Math.min(elapsed / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        el.textContent = Math.round(target * eased);
        if (progress < 1) requestAnimationFrame(update);
      }
      requestAnimationFrame(update);
      counterObserver.unobserve(el);
    }
  });
}, { threshold: 0.5 });
counters.forEach(c => counterObserver.observe(c));

// ============ PARTICULES CONNECTEES ============
const canvas = document.getElementById('bgCanvas');
if (canvas) {
  const ctx = canvas.getContext('2d');
  let particles = [];
  let width = canvas.width = window.innerWidth;
  let height = canvas.height = window.innerHeight;
  let mouse = { x: -1000, y: -1000 };

  const PARTICLE_COUNT = 50;
  const MAX_DISTANCE = 130;
  const MOUSE_RADIUS = 150;

  class Particle {
    constructor() {
      this.x = Math.random() * width;
      this.y = Math.random() * height;
      this.vx = (Math.random() - 0.5) * 0.3;
      this.vy = (Math.random() - 0.5) * 0.3;
      this.radius = Math.random() * 1.8 + 0.6;
    }
    update() {
      const dx = this.x - mouse.x;
      const dy = this.y - mouse.y;
      const dist = Math.sqrt(dx * dx + dy * dy);
      if (dist < MOUSE_RADIUS && dist > 0) {
        const force = (MOUSE_RADIUS - dist) / MOUSE_RADIUS;
        this.vx += (dx / dist) * force * 0.3;
        this.vy += (dy / dist) * force * 0.3;
      }

      this.x += this.vx;
      this.y += this.vy;

      this.vx *= 0.99;
      this.vy *= 0.99;

      if (this.x < 0 || this.x > width) { this.vx *= -1; this.x = Math.max(0, Math.min(width, this.x)); }
      if (this.y < 0 || this.y > height) { this.vy *= -1; this.y = Math.max(0, Math.min(height, this.y)); }
    }
    draw() {
      ctx.beginPath();
      ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(167, 139, 250, 0.6)';
      ctx.fill();
    }
  }

  for (let i = 0; i < PARTICLE_COUNT; i++) {
    particles.push(new Particle());
  }

  document.addEventListener('mousemove', (e) => {
    mouse.x = e.clientX;
    mouse.y = e.clientY;
  }, { passive: true });

  document.addEventListener('mouseleave', () => {
    mouse.x = -1000;
    mouse.y = -1000;
  });

  function connectParticles() {
    for (let i = 0; i < particles.length; i++) {
      for (let j = i + 1; j < particles.length; j++) {
        const dx = particles[i].x - particles[j].x;
        const dy = particles[i].y - particles[j].y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        if (distance < MAX_DISTANCE) {
          const opacity = (1 - distance / MAX_DISTANCE) * 0.35;
          ctx.beginPath();
          ctx.strokeStyle = 'rgba(124, 92, 255, ' + opacity + ')';
          ctx.lineWidth = 1;
          ctx.moveTo(particles[i].x, particles[i].y);
          ctx.lineTo(particles[j].x, particles[j].y);
          ctx.stroke();
        }
      }
    }
  }

  function animateParticles() {
    ctx.clearRect(0, 0, width, height);
    particles.forEach(p => { p.update(); p.draw(); });
    connectParticles();
    requestAnimationFrame(animateParticles);
  }
  animateParticles();

  window.addEventListener('resize', () => {
    width = canvas.width = window.innerWidth;
    height = canvas.height = window.innerHeight;
  });
}

// ============ SMOOTH SCROLL ============
document.querySelectorAll('a[href^="#"]').forEach(link => {
  link.addEventListener('click', e => {
    const href = link.getAttribute('href');
    if (href === '#') return;
    const target = document.querySelector(href);
    if (target) {
      e.preventDefault();
      const top = target.getBoundingClientRect().top + window.scrollY - 80;
      window.scrollTo({ top, behavior: 'smooth' });
    }
  });
});

// ============ RIPPLE ============
const rippleStyle = document.createElement('style');
rippleStyle.textContent = '@keyframes rippleAnim { to { transform: scale(1); opacity: 0; } }';
document.head.appendChild(rippleStyle);

document.querySelectorAll('[data-ripple]').forEach(btn => {
  btn.addEventListener('click', (e) => {
    const rect = btn.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const size = Math.max(rect.width, rect.height) * 2;

    const ripple = document.createElement('span');
    ripple.style.cssText =
      'position: absolute;' +
      'left: ' + (x - size / 2) + 'px;' +
      'top: ' + (y - size / 2) + 'px;' +
      'width: ' + size + 'px;' +
      'height: ' + size + 'px;' +
      'background: rgba(255,255,255,0.3);' +
      'border-radius: 50%;' +
      'pointer-events: none;' +
      'transform: scale(0);' +
      'animation: rippleAnim 0.6s ease-out;';

    btn.appendChild(ripple);
    setTimeout(() => ripple.remove(), 700);
  });
});

// ============ CONFETTI (easter egg) ============
let clickCount = 0;
document.addEventListener('click', (e) => {
  if (!e.target.closest('a') && !e.target.closest('button')) {
    clickCount++;
    if (clickCount % 5 === 0) {
      launchConfetti(e.clientX, e.clientY);
    }
  }
});

function launchConfetti(x, y) {
  const colors = ['#7C5CFF', '#A78BFA', '#64DCFF', '#4ADE80', '#FBBF24'];
  for (let i = 0; i < 20; i++) {
    const el = document.createElement('div');
    const color = colors[Math.floor(Math.random() * colors.length)];
    el.style.cssText =
      'position: fixed;' +
      'left: ' + x + 'px;' +
      'top: ' + y + 'px;' +
      'width: 8px;' +
      'height: 8px;' +
      'background: ' + color + ';' +
      'border-radius: ' + (Math.random() < 0.5 ? '50%' : '2px') + ';' +
      'pointer-events: none;' +
      'z-index: 99999;' +
      'transition: all 1.2s cubic-bezier(0.16, 1, 0.3, 1);';

    document.body.appendChild(el);

    const angle = (Math.PI * 2 * i) / 20 + Math.random() * 0.5;
    const velocity = 100 + Math.random() * 200;
    const tx = Math.cos(angle) * velocity;
    const ty = Math.sin(angle) * velocity - 100;

    requestAnimationFrame(() => {
      el.style.transform = 'translate(' + tx + 'px, ' + ty + 'px) rotate(' + (Math.random() * 720) + 'deg)';
      el.style.opacity = '0';
    });

    setTimeout(() => el.remove(), 1300);
  }
}

// ============ CONSOLE ============
console.log('%c Arzhe', 'font-size: 32px; font-weight: 900; color: #A78BFA;');
console.log('%cSite v2.0.1 - Built with everything', 'font-size: 14px; color: #64DCFF;');
console.log('%cAstuce : clique 5 fois n\'importe ou pour lancer des confetti !', 'font-size: 12px; color: #64DCFF;');