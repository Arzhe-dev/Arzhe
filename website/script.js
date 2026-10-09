// ============================================================
//  ARZHE - Site Web v2.0 (niveau Apple)
// ============================================================

// ------------------------------------------------------------
//  1. CURSEUR CUSTOM
// ------------------------------------------------------------
const cursor = document.getElementById('cursor');
const cursorDot = document.getElementById('cursorDot');
let mouseX = 0, mouseY = 0;
let cursorX = 0, cursorY = 0;

document.addEventListener('mousemove', (e) => {
  mouseX = e.clientX;
  mouseY = e.clientY;
  if (cursorDot) {
    cursorDot.style.transform = `translate(${mouseX - 3}px, ${mouseY - 3}px)`;
  }
});

function animateCursor() {
  cursorX += (mouseX - cursorX) * 0.15;
  cursorY += (mouseY - cursorY) * 0.15;
  if (cursor) {
    cursor.style.transform = `translate(${cursorX - 16}px, ${cursorY - 16}px)`;
  }
  requestAnimationFrame(animateCursor);
}
animateCursor();

// Hover sur les elements interactifs
document.querySelectorAll('a, button, .feature-card, .screen-card, .step-card, .showcase-mockup').forEach(el => {
  el.addEventListener('mouseenter', () => cursor && cursor.classList.add('hover'));
  el.addEventListener('mouseleave', () => cursor && cursor.classList.remove('hover'));
});

// ------------------------------------------------------------
//  2. SCROLL PROGRESS
// ------------------------------------------------------------
const scrollProgress = document.getElementById('scrollProgress');
const nav = document.getElementById('nav');

window.addEventListener('scroll', () => {
  const scrollTop = window.scrollY;
  const docHeight = document.documentElement.scrollHeight - window.innerHeight;
  const scrollPercent = (scrollTop / docHeight) * 100;

  if (scrollProgress) {
    scrollProgress.style.width = scrollPercent + '%';
  }
  if (nav) {
    if (scrollTop > 50) nav.classList.add('scrolled');
    else nav.classList.remove('scrolled');
  }
});

// ------------------------------------------------------------
//  3. REVEAL AU SCROLL
// ------------------------------------------------------------
const revealObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      entry.target.classList.add('visible');
      revealObserver.unobserve(entry.target);
    }
  });
}, { threshold: 0.15, rootMargin: '0px 0px -100px 0px' });

document.querySelectorAll('.reveal').forEach(el => revealObserver.observe(el));

// ------------------------------------------------------------
//  4. PARALLAX
// ------------------------------------------------------------
const parallaxElements = document.querySelectorAll('[data-parallax]');

window.addEventListener('scroll', () => {
  const scrollY = window.scrollY;
  parallaxElements.forEach(el => {
    const speed = parseFloat(el.getAttribute('data-parallax')) || 0.2;
    const offset = scrollY * speed;
    el.style.transform = `translateY(${offset}px)`;
  });
});

// ------------------------------------------------------------
//  5. BOUTONS MAGNETIC
// ------------------------------------------------------------
document.querySelectorAll('.magnetic').forEach(btn => {
  btn.addEventListener('mousemove', (e) => {
    const rect = btn.getBoundingClientRect();
    const x = e.clientX - rect.left - rect.width / 2;
    const y = e.clientY - rect.top - rect.height / 2;
    btn.style.transform = `translate(${x * 0.25}px, ${y * 0.25}px)`;
  });
  btn.addEventListener('mouseleave', () => {
    btn.style.transform = '';
  });
});

// ------------------------------------------------------------
//  6. TILT 3D SUR LES CARTES
// ------------------------------------------------------------
document.querySelectorAll('[data-tilt]').forEach(card => {
  card.addEventListener('mousemove', (e) => {
    const rect = card.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const centerX = rect.width / 2;
    const centerY = rect.height / 2;
    const rotateX = ((y - centerY) / centerY) * -6;
    const rotateY = ((x - centerX) / centerX) * 6;

    card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-8px)`;
  });
  card.addEventListener('mouseleave', () => {
    card.style.transform = '';
  });
});

// ------------------------------------------------------------
//  7. CANVAS PARTICULES CONNECTEES
// ------------------------------------------------------------
const canvas = document.getElementById('bgCanvas');
if (canvas) {
  const ctx = canvas.getContext('2d');
  let particles = [];
  let width = canvas.width = window.innerWidth;
  let height = canvas.height = window.innerHeight;

  const PARTICLE_COUNT = 50;
  const MAX_DISTANCE = 140;

  class Particle {
    constructor() {
      this.x = Math.random() * width;
      this.y = Math.random() * height;
      this.vx = (Math.random() - 0.5) * 0.4;
      this.vy = (Math.random() - 0.5) * 0.4;
      this.radius = Math.random() * 2 + 1;
    }
    update() {
      this.x += this.vx;
      this.y += this.vy;
      if (this.x < 0 || this.x > width) this.vx *= -1;
      if (this.y < 0 || this.y > height) this.vy *= -1;
    }
    draw() {
      ctx.beginPath();
      ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(167, 139, 250, 0.5)';
      ctx.fill();
    }
  }

  for (let i = 0; i < PARTICLE_COUNT; i++) {
    particles.push(new Particle());
  }

  function connectParticles() {
    for (let i = 0; i < particles.length; i++) {
      for (let j = i + 1; j < particles.length; j++) {
        const dx = particles[i].x - particles[j].x;
        const dy = particles[i].y - particles[j].y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        if (distance < MAX_DISTANCE) {
          const opacity = (1 - distance / MAX_DISTANCE) * 0.4;
          ctx.beginPath();
          ctx.strokeStyle = `rgba(124, 92, 255, ${opacity})`;
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

// ------------------------------------------------------------
//  8. GRADIENT ANIME DANS LE HERO
// ------------------------------------------------------------
const heroGradient = document.getElementById('heroGradient');
if (heroGradient) {
  let angle = 0;
  setInterval(() => {
    angle = (angle + 1) % 360;
    heroGradient.style.filter = `hue-rotate(${Math.sin(angle * Math.PI / 180) * 15}deg)`;
  }, 80);
}

// ------------------------------------------------------------
//  9. SMOOTH SCROLL
// ------------------------------------------------------------
document.querySelectorAll('a[href^="#"]').forEach(link => {
  link.addEventListener('click', e => {
    const target = document.querySelector(link.getAttribute('href'));
    if (target) {
      e.preventDefault();
      target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  });
});

console.log('%c🌙 Arzhe', 'font-size: 24px; font-weight: bold; color: #A78BFA;');
console.log('%cSite v2.0 — Niveau Apple', 'font-size: 14px; color: #7C5CFF;');