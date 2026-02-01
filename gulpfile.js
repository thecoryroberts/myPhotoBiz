const gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const postcss = require('gulp-postcss');
const autoprefixer = require('autoprefixer');
const cssnano = require('cssnano');
const sourcemaps = require('gulp-sourcemaps');

const paths = {
    styles: {
        // Compile the app entrypoint to avoid compiling partials directly
        src: 'wwwroot/scss/app.scss',
        dest: 'wwwroot/css'
    }
};

function styles() {
    return gulp.src(paths.styles.src)
        .pipe(sourcemaps.init())
        .pipe(sass({ quietDeps: true, includePaths: ['node_modules'] }).on('error', sass.logError))
        .pipe(postcss([
            autoprefixer(),
            cssnano()
        ]))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(paths.styles.dest));
}

function watch() {
    gulp.watch(paths.styles.src, styles);
}

exports.styles = styles;
exports.watch = watch;
exports.default = gulp.series(styles, watch);